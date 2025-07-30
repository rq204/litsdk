using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace litsdk
{
    /// <summary>
    /// 流程配置文件
    /// </summary>
    public class Project
    {
        public Project()
        {
            this.Id = Guid.NewGuid();
            this.Nodes = new List<Node>();
            this.Variables = new List<litsdk.Variable>();
        }

        public Guid Id { get; set; }

        /// <summary>
        /// 所有配置节点
        /// </summary>
        public List<Node> Nodes { get; set; }

        /// <summary>
        /// 所有的变量
        /// </summary>
        public List<litsdk.Variable> Variables { get; set; }

        /// <summary>
        /// 流程授权信息
        /// </summary>
        public string Auth { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// 流程作者信息
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// 输入参数，为字符，数字，列表三种
        /// </summary>
        public List<string> InputVars { get; set; } = new List<string>();

        /// <summary>
        /// 输出参数，为字符，数字，列表三种
        /// </summary>
        public List<string> OutPutVars { get; set; } = new List<string>();

        /// <summary>
        /// 流程的描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 按节点Id获取对应Node
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private Node GetNodeById(Guid id)
        {
            return this.Nodes.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// 当前流程生成Json字符
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    return JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                }
                catch
                {
                    System.Threading.Thread.Sleep(200);
                }
            }
            throw new Exception("序列化流程对象错误");
        }

        /// <summary>
        /// 清空所有变量
        /// </summary>
        public void ClearVariables()
        {
            foreach (litsdk.Variable v in this.Variables)
            {
                v.IntValue = 0;
                v.ListValue = new List<string>();
                v.TableValue = new System.Data.DataTable();
                v.StrValue = "";
            }
        }

        /// <summary>
        /// 解析流程Json配置错误的原因
        /// </summary>
        public static Newtonsoft.Json.Serialization.ErrorEventArgs PaserError = null;

        /// <summary>
        /// 解析流程
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Project GetProject(string json)
        {
            try
            {
                Project pj = JsonConvert.DeserializeObject<Project>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, Error = new EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs>((a, b) => PaserError = b) });
                return pj;
            }
            catch
            {
                return null;
            }
        }
    }
}