using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSRedis;
using litsdk;

namespace litredis
{
    [litsdk.Action(Name = "Redis", Category = "数据库", IsLinux = true, RefFile = "CSRedisCore.dll", Order = 50, Description = "通过Redis进行数据的读取和写入删除等操作")] 
    public class RedisActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "Redis";


        [Argument(Name = "操作方式", ControlType = ControlType.ComboBox, Order = 1, Description = "如何操作Redis")]
        public RedisType RedisType { get; set; } = RedisType.set;

        /// <summary>
        /// 服务器地址
        /// </summary>
        [Argument(Name = "服务器地址", ControlType = ControlType.TextBox, Order = 2, Description = "服务器地址，可以为IP地址或是域名")]
        public string Host { get; set; } = "";

        /// <summary>
        /// 密码
        /// </summary>
        [Argument(Name = "密码", ControlType = ControlType.Password, Order = 3, Description = "登录密码")]
        public string Password { get; set; } = "";

        /// <summary>
        /// 数据库
        /// </summary>
        [Argument(Name = "数据库", ControlType = ControlType.NumericUpDown, Order = 4, Description = "从1-256")]
        public int DatabaseNum { get; set; } = 0;

        /// <summary>
        /// 变量key名
        /// </summary>
        [Argument(Name = "操作键名", ControlType = ControlType.TextBox, Order = 5, Description = "要操作的Key名称")]
        public string Key { get; set; } = "";


        static Dictionary<string, CSRedisClient> redisDic = new Dictionary<string, CSRedisClient>();
        static object redislock = new object();

        /// <summary>
        /// 变量值
        /// </summary>
        [Argument(Name = "写入变量", ControlType = ControlType.Variable, Order = 6, Description = "将该变量的值写入Redis")]
        public string ValueVarName { get; set; }

        [Argument(Name = "移除键值", ControlType = ControlType.TextBox, Order = 6, Description = "将指定集合的键值移除")]
        public string RemoveValue { get; set; }

        /// <summary>
        /// 操作结果变量名
        /// </summary>
        [Argument(Name = "结果存入", ControlType = ControlType.Variable, Order = 8, Description = "将redis查询或操作的结果存入变量")]
        public string ResultVarName { get; set; }

        public override void Execute(ActivityContext context)
        {
            string host = context.ReplaceVar(this.Host);
            string password = context.ReplaceVar(this.Password);
            string key = context.ReplaceVar(this.Key);

            string conn = $"{host},password={password},defaultDatabase={this.DatabaseNum},poolsize=5,ssl=false";

            CSRedisClient redisManger = redisDic.ContainsKey(conn) ? redisDic[conn] : null;

            if (redisManger == null)
            {
                lock (redislock)
                {
                    redisManger = new CSRedisClient(conn);
                    if (!redisDic.ContainsKey(conn))
                    {
                        redisDic.Add(conn, redisManger);
                    }
                }
            }

            List<string> pushs = new List<string>();
            if (this.RedisType == RedisType.lpush || this.RedisType == RedisType.rpush || this.RedisType == RedisType.set || this.RedisType == RedisType.sadd)
            {
                if (context.ContainsStr(this.ValueVarName)) pushs.Add(context.GetStr(this.ValueVarName));
                else if (context.ContainsList(this.ValueVarName)) pushs.AddRange(context.GetList(this.ValueVarName));
            }
            string result = null;
            switch (this.RedisType)
            {
                case RedisType.set:
                    if (redisManger.Set(key, pushs))
                    {
                        context.WriteLog($"字符{key}成功写值长度{CutShow(string.Join(",", pushs.ToArray()))}");
                        result = "1";
                    }
                    else
                    {
                        context.WriteLog($"字符{key}写值失败");
                        result = "0";
                    }
                    break;
                case RedisType.get:
                    context.SetVarStr(this.ResultVarName, "");
                    string value = redisManger.Get(key);
                    if (value == null) value = "";
                    result = value;
                    context.WriteLog($"键名 {key} 取值成功 {CutShow(value)}");
                    break;
                case RedisType.lpush:
                    long l = redisManger.LPush<string>(key, pushs.ToArray());
                    result = l.ToString();
                    context.WriteLog($"键名 {key} lpush成功");
                    break;
                case RedisType.rpush:
                    long l2 = redisManger.RPush<string>(key, pushs.ToArray());
                    result = l2.ToString();
                    context.WriteLog($"键名 {key} rpush成功");
                    break;
                case RedisType.rpop:
                    context.SetVarStr(this.ResultVarName, "");
                    string rvalue = redisManger.RPop(key);
                    if (rvalue == null) rvalue = "";
                    result = rvalue;
                    context.WriteLog($"键名 {key}rpush 成功 {CutShow(rvalue)}");
                    break;
                case RedisType.lpop:
                    context.SetVarStr(this.ResultVarName, "");
                    string lvalue = redisManger.LPop(key);
                    if (lvalue == null) lvalue = "";
                    result = lvalue;
                    context.WriteLog($"键名 {key} lpop成功 {CutShow(lvalue)}");
                    break;
                case RedisType.sadd:
                    result = redisManger.SAdd<string>(key, pushs.ToArray()).ToString();
                    context.WriteLog($"键名 {key} sadd 值  {string.Join(",", pushs.ToArray())}成功 {CutShow(result)}");
                    break;
                case RedisType.sismember:
                    string sis = context.GetStr(this.ValueVarName);
                    result = redisManger.SIsMember(key, sis) ? "1" : "0";
                    context.WriteLog($"键名 {key} sismember 值  {sis} 结果为 {result}");
                    break;
                case RedisType.srem:
                    string srvalue = context.ReplaceVar(this.RemoveValue);
                    long srlong = redisManger.SRem<string>(key, new string[] { srvalue });
                    context.WriteLog($"键名 {key} srem 成功 {CutShow(srvalue)}");
                    break;
            }
            if (!string.IsNullOrEmpty(this.ResultVarName))
            {
                context.SetVarStr(this.ResultVarName, result);
            }
        }

        public static string CutShow(string txt, int len = 10)
        {
            if (txt.Length > len) return $"长度{txt.Length}";
            else return txt;
        }

        public override void Validate(ActivityContext context)
        {
            if (string.IsNullOrEmpty(this.Host)) throw new Exception("服务器地址不能为空");
            if (string.IsNullOrEmpty(this.Key)) throw new Exception("键名不能为空");

            ///这些是要设置键值
            if (this.RedisType == RedisType.set || this.RedisType == RedisType.lpush || this.RedisType == RedisType.rpush || this.RedisType == RedisType.sadd || this.RedisType == RedisType.sismember)
            {
                if (string.IsNullOrEmpty(this.ValueVarName)) throw new Exception("键值不能为空");
                if (!context.ContainsStr(this.ValueVarName) && !context.ContainsList(this.ValueVarName)) throw new Exception($"不存在字符或列表变量：{this.ValueVarName}");
            }

            if (string.IsNullOrEmpty(this.ValueVarName) && string.IsNullOrEmpty(this.ResultVarName)) throw new Exception("键值读取和写入变量不能同时为空");

            //这些获取的值为string
            if (this.RedisType == RedisType.get || this.RedisType == RedisType.lpop || this.RedisType == RedisType.rpop)
            {
                if (!context.ContainsStr(this.ResultVarName))
                {
                    throw new Exception("当前要保存数据至字符变量");
                }
            }

            //键值类型
            if (this.RedisType == RedisType.lpush || this.RedisType == RedisType.rpush || this.RedisType == RedisType.set || this.RedisType == RedisType.sadd)
            {
                if (!context.ContainsStr(this.ValueVarName) && !context.ContainsList(this.ValueVarName)) throw new Exception($"不存在字符或列表变量：{this.ValueVarName}");
            }
            else if (this.RedisType == RedisType.get || this.RedisType == RedisType.lpop || this.RedisType == RedisType.rpop)//不需要
            {

            }
            else if (this.RedisType == RedisType.sadd || this.RedisType == RedisType.sismember)
            {
                if (!context.ContainsStr(this.ValueVarName)) throw new Exception($"不存在字符变量：{this.ValueVarName}");
            }
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle();
            switch (field)
            {
                case "ValueVarName":
                    switch (this.RedisType)
                    {
                        case RedisType.set:
                        case RedisType.lpush:
                        case RedisType.rpush:
                        case RedisType.sadd:
                        case RedisType.sismember:
                            style.Variables = litsdk.ControlStyle.GetVariables(true, true);
                            style.Visible = true;
                            break;
                        default:
                            style.Visible = false;
                            break;
                    }
                    break;
                case "ResultVarName":
                    style.Variables = ControlStyle.GetVariables(true);
                    if (this.RedisType == RedisType.srem) style.Visible = false;
                    break;
                case "RemoveValue":
                    style.Visible = this.RedisType == RedisType.srem;
                    style.Variables = ControlStyle.GetVariables(true, false, true);
                    break;
            }

            return style;
        }
    }
}