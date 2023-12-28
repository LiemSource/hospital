using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VaeEntity.Hospital
{
    public enum ReservationStatus
    {
        [Description("新任务")]
        New,
        [Description("执行中")]
        Excuting,
        [Description("已成功")]
        Successed,
        [Description("已失败")]
        Failed,
    }
}
