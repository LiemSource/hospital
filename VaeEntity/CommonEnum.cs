using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace VaeEntity
{
   
    public enum Status
    {
        [Description("禁用")]
        Disable,
        [Description("启用")]
        Enable,
        [Description("删除")]
        Delete

    }
    public enum RetCode
    {
        [Description("操作成功")]
        BizOK = 0,

        [Description("成功")]
        SignSuccess = 1,
        /// <summary>
        /// 操作失败
        /// </summary>
        [Description("操作失败")]
        BizError = -1,
    }

}
