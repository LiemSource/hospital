using Autofac;
using Autofac.Extensions.DependencyInjection;
using BotHelper;
using Hangfire;
using Hangfire.MySql.Core;
using HangfireBasicAuthenticationFilter;
using HospitalJob.Jobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Data;
using VaeDbContext;
using VaeHelper;


using Microsoft.AspNetCore.Http;
using System.Net;

namespace HospitalJob
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(configuration => configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseFilter(new NotReentryServerHangfireFilter())
                    .UseStorage(new MySqlStorage(Configuration["ConnectionStrings:HangfireConnection"], new MySqlStorageOptions
                    {
                        TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                        QueuePollInterval = TimeSpan.FromSeconds(15),       //- 作业队列轮询间隔。默认值为15秒。
                        JobExpirationCheckInterval = TimeSpan.FromHours(1), //- 作业到期检查间隔（管理过期记录）。默认值为1小时。
                        CountersAggregateInterval = TimeSpan.FromMinutes(5), //- 聚合计数器的间隔。默认为5分钟。
                        PrepareSchemaIfNecessary = true, //- 如果设置为true，则创建数据库表。默认是true。
                        DashboardJobListLimit = 50000, //- 仪表板作业列表限制。默认值为50000。
                        TransactionTimeout = TimeSpan.FromMinutes(1), //- 交易超时。默认为1分钟。
                        TablePrefix = "Hangfire"
                    })));

            services.AddHangfireServer();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            services.AddMvc(o => o.EnableEndpointRouting = false);
            services.AddDbContext<HospitalContext>();
            services.AddTransient<ProxyHelper>();
            services.AddSingleton<HospitalHelper>();
            services.AddSingleton<PatientHelper>();
            services.AddTransient<InPatientQueryService>();
            services.AddTransient<OutPatientQueryService>();
            services.AddTransient<MedicationQueryService>();
            services.AddTransient<OutPatientMedicationQueryService>();
            services.AddTransient<QueryDoctScheduleService>();
            services.AddTransient<QueryOutPatientEmrPDFService>();
            services.AddTransient<OutpatientEmptyMedicationResetService>();
            //services.AddTransient<InPatientHistoryQueryService>();
            services.AddTransient<PatientVisitReviewedJob>();
            services.AddTransient<TelegramBotHelper>();
            services.AddTransient<InPatientHistoryQueryService>();
            var builder = new ContainerBuilder();
            builder.Populate(services);
            return new AutofacServiceProvider(builder.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILifetimeScope lifetimeScope)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            //app.UseRouting();

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapGet("/", async context =>
            //    {
            //        await context.Response.WriteAsync("Hello World!");
            //    });
            //});

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();

            //Hangfire图形可视化面板
            app.UseHangfireServer();
            app.UseHangfireDashboard("/jobs", new DashboardOptions
            {
                //访问面板需要登录，此处也可以不设置
                Authorization = new[] { new HangfireCustomBasicAuthenticationFilter { User = "friday", Pass = "liem@hangjob" } }
            });


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            RecurringJob.AddOrUpdate<InPatientQueryService>("PatientTask", r => r.Excute(null), "0 30 12,17,22 * * ?", TimeZoneInfo.Local);//每天晚上22点30分
            RecurringJob.AddOrUpdate<MedicationQueryService>("MedicationTask", r => r.Excute(null), "0 30 23 ? * MON", TimeZoneInfo.Local);//每周一晚上23点30分
            RecurringJob.AddOrUpdate<MedicationQueryService>("RetryMedicationTask", r => r.QueryMedications(true, default(DateTime), default(DateTime)), "0 0 0 * * ?", TimeZoneInfo.Local);//每天0点0分 
            RecurringJob.AddOrUpdate<OutPatientQueryService>("OutPatientTask", r => r.Excute(null), "0 30 19 * * ?", TimeZoneInfo.Local);//每天晚上19点30分
            RecurringJob.AddOrUpdate<QueryDoctScheduleService>("QueryDoctScheduleTask", r => r.Excute(null), "0 0 8,10,12,14,16,18,20 * * ?", TimeZoneInfo.Local);//每天晚上19点00分
            RecurringJob.AddOrUpdate<OutPatientMedicationQueryService>("OutPatientMedicationTask", r => r.Excute(null), "0 00 23 * * ?", TimeZoneInfo.Local);//每天晚上23点00分
            RecurringJob.AddOrUpdate<PatientVisitReviewedJob>("PatientVisitReviewedTask", r => r.Excute(null), "0 0 20 L * ?", TimeZoneInfo.Local);//每月最后一天晚上8点

            //RecurringJob.AddOrUpdate<InPatientHistoryQueryService>("InPatientHistoryQueryTask", r => r.Excute(null), "0 00 8 * * ?", TimeZoneInfo.Local);//每天上午8点00分
            //BackgroundJob.Enqueue<OutPatientMedicationQueryService>(r => r.QueryOutPatientMedications(new DateTime(2022, 01, 01), new DateTime(2022, 03, 01), 3, 0));
        }
    }
}


////支持基于队列的任务处理：任务执行不是同步的，而是放到一个持久化队列中，以便马上把请求控制权返回给调用者。
//var jobId = BackgroundJob.Enqueue(() => WriteLog("队列任务执行了！"));

////延迟任务执行：不是马上调用方法，而是设定一个未来时间点再来执行，延迟作业仅执行一次
//var jobId = BackgroundJob.Schedule（()=>WriteLog("一天后的延迟任务执行了！"),TimeSpan.FromDays(1));//一天后执行该任务

////循环任务执行：一行代码添加重复执行的任务，其内置了常见的时间循环模式，也可基于CRON表达式来设定复杂的模式。【用的比较的多】
//RecurringJob.AddOrUpdate(() => WriteLog("每分钟执行任务！"), Cron.Minutely); //注意最小单位是分钟

////延续性任务执行：类似于.NET中的Task,可以在第一个任务执行完之后紧接着再次执行另外的任务
//BackgroundJob.ContinueWith(jobId, () => WriteLog("连续任务！"));

////不调用方法，仅输出测试
//RecurringJob.AddOrUpdate("每4时执行一次", () => Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")), "0 0 */4 * * ?", TimeZoneInfo.Local);