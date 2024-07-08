using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace litnosql
{
    public enum MongodbCmdType
    {
        [Description("插入一条新记录")]
        Insert = 0,
        [Description("删除符合条件的所有记录")]
        Delete = 1,
        [Description("更新符合条件的所有记录")]
        Update = 2,
        [Description("查询记录")]
        Select = 3
    }
}