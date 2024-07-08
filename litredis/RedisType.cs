using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace litredis
{
    public enum RedisType
    {
        /// <summary>
        /// 设置key值 
        /// </summary>
        [Description("set，设置key值")]
        set = 0,
        /// <summary>
        /// 获取key值
        /// </summary>
        [Description("get，获取key值")]
        get = 1,
        /// <summary>
        /// 将一个或多个值 value 插入到列表 key 的表头
        /// </summary>
        [Description("lpush，将一个或多个值 value 插入到列表 key 的表头")]
        lpush = 2,
        /// <summary>
        /// 将一个或多个值 value 插入到列表 key 的表尾(最右边)
        /// </summary>
        [Description("rpush，将一个或多个值 value 插入到列表 key 的表尾(最右边)")]
        rpush = 3,
        /// <summary>
        /// 移除并返回列表 key 的头元素
        /// </summary>
        [Description("lpop，移除并返回列表 key 的头元素")]
        lpop = 4,
        /// <summary>
        /// 移除并返回列表 key 的尾元素
        /// </summary>
        [Description("rpop，移除并返回列表 key 的尾元素")]
        rpop = 5,
        /// <summary>
        /// 添加集合，返回大于0为成功，0为已存在
        /// </summary>
        [Description("sadd，添加集合，返回大于0为成功，0为已存在")]
        sadd = 6,
        /// <summary>
        /// 查看是否存在，0不存在1存在
        /// </summary>
        [Description("sismember，查看是否存在，返回0为不存在1为存在")]
        sismember = 7,
        /// <summary>
        /// 移除值
        /// </summary>
        [Description("srem，移除集合中的指定值")]
        srem = 8
    }
}
