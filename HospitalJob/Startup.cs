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
                        QueuePollInterval = TimeSpan.FromSeconds(15),       //- ��ҵ������ѯ�����Ĭ��ֵΪ15�롣
                        JobExpirationCheckInterval = TimeSpan.FromHours(1), //- ��ҵ���ڼ������������ڼ�¼����Ĭ��ֵΪ1Сʱ��
                        CountersAggregateInterval = TimeSpan.FromMinutes(5), //- �ۺϼ������ļ����Ĭ��Ϊ5���ӡ�
                        PrepareSchemaIfNecessary = true, //- �������Ϊtrue���򴴽����ݿ��Ĭ����true��
                        DashboardJobListLimit = 50000, //- �Ǳ����ҵ�б����ơ�Ĭ��ֵΪ50000��
                        TransactionTimeout = TimeSpan.FromMinutes(1), //- ���׳�ʱ��Ĭ��Ϊ1���ӡ�
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

            //Hangfireͼ�ο��ӻ����
            app.UseHangfireServer();
            app.UseHangfireDashboard("/jobs", new DashboardOptions
            {
                //���������Ҫ��¼���˴�Ҳ���Բ�����
                Authorization = new[] { new HangfireCustomBasicAuthenticationFilter { User = "friday", Pass = "liem@hangjob" } }
            });


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            RecurringJob.AddOrUpdate<InPatientQueryService>("PatientTask", r => r.Excute(null), "0 30 12,17,22 * * ?", TimeZoneInfo.Local);//ÿ������22��30��
            RecurringJob.AddOrUpdate<MedicationQueryService>("MedicationTask", r => r.Excute(null), "0 30 23 ? * MON", TimeZoneInfo.Local);//ÿ��һ����23��30��
            RecurringJob.AddOrUpdate<MedicationQueryService>("RetryMedicationTask", r => r.QueryMedications(true, default(DateTime), default(DateTime)), "0 0 0 * * ?", TimeZoneInfo.Local);//ÿ��0��0�� 
            RecurringJob.AddOrUpdate<OutPatientQueryService>("OutPatientTask", r => r.Excute(null), "0 30 19 * * ?", TimeZoneInfo.Local);//ÿ������19��30��
            RecurringJob.AddOrUpdate<QueryDoctScheduleService>("QueryDoctScheduleTask", r => r.Excute(null), "0 0 8,10,12,14,16,18,20 * * ?", TimeZoneInfo.Local);//ÿ������19��00��
            RecurringJob.AddOrUpdate<OutPatientMedicationQueryService>("OutPatientMedicationTask", r => r.Excute(null), "0 00 23 * * ?", TimeZoneInfo.Local);//ÿ������23��00��
            RecurringJob.AddOrUpdate<PatientVisitReviewedJob>("PatientVisitReviewedTask", r => r.Excute(null), "0 0 20 L * ?", TimeZoneInfo.Local);//ÿ�����һ������8��

            //RecurringJob.AddOrUpdate<InPatientHistoryQueryService>("InPatientHistoryQueryTask", r => r.Excute(null), "0 00 8 * * ?", TimeZoneInfo.Local);//ÿ������8��00��
            //BackgroundJob.Enqueue<OutPatientMedicationQueryService>(r => r.QueryOutPatientMedications(new DateTime(2022, 01, 01), new DateTime(2022, 03, 01), 3, 0));
        }
    }
}


////֧�ֻ��ڶ��е�����������ִ�в���ͬ���ģ����Ƿŵ�һ���־û������У��Ա����ϰ��������Ȩ���ظ������ߡ�
//var jobId = BackgroundJob.Enqueue(() => WriteLog("��������ִ���ˣ�"));

////�ӳ�����ִ�У��������ϵ��÷����������趨һ��δ��ʱ�������ִ�У��ӳ���ҵ��ִ��һ��
//var jobId = BackgroundJob.Schedule��()=>WriteLog("һ�����ӳ�����ִ���ˣ�"),TimeSpan.FromDays(1));//һ���ִ�и�����

////ѭ������ִ�У�һ�д�������ظ�ִ�е������������˳�����ʱ��ѭ��ģʽ��Ҳ�ɻ���CRON���ʽ���趨���ӵ�ģʽ�����õıȽϵĶࡿ
//RecurringJob.AddOrUpdate(() => WriteLog("ÿ����ִ������"), Cron.Minutely); //ע����С��λ�Ƿ���

////����������ִ�У�������.NET�е�Task,�����ڵ�һ������ִ����֮��������ٴ�ִ�����������
//BackgroundJob.ContinueWith(jobId, () => WriteLog("��������"));

////�����÷��������������
//RecurringJob.AddOrUpdate("ÿ4ʱִ��һ��", () => Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")), "0 0 */4 * * ?", TimeZoneInfo.Local);