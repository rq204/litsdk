using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using litsdk;
using MongoDB.Bson;
using MongoDB.Driver;

namespace litnosql
{
    [litsdk.Action(Name = "MongoDB", RefFile = "MongoDB.Bson.dll,MongoDB.Driver.dll,MongoDB.Driver.Core.dll,MongoDB.Libmongocrypt.dll,DnsClient.dll,SharpCompress.dll", IsLinux = true, Category = "数据库", IsFront = false, Order = 300, Description = "支持添加、删除、更新、查询操作")] 
    /// <summary>
    /// MongoDB
    /// </summary>
    public class MongoDBActivity : litsdk.ProcessActivity
    {
        public override string Name { get; set; } = "MongoDB";

        /// <summary>
        /// 服务器地址
        /// </summary>
        [litsdk.Argument(Name = "主机地址", ControlType = ControlType.TextBox, Order = 1, Description = "服务器地址")]
        public string ServerConn { get; set; }

        /// <summary>
        /// 数据库
        /// </summary>
        [litsdk.Argument(Name = "数据库", ControlType = ControlType.TextBox, Order = 2, Description = "要操作的数据库名称")]
        public string DataBase { get; set; }

        /// <summary>
        /// 集合名
        /// </summary>
        [litsdk.Argument(Name = "集合名", Order = 3, ControlType = ControlType.TextBox, Description = "集合名称")]
        public string Collection { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        [Argument(Name = "操作方式", ControlType = ControlType.ComboBox, Order = 5, Description = "操作类型")]
        public MongodbCmdType MongodbCmdType { get; set; } = MongodbCmdType.Insert;

        /// <summary>
        /// 操作的对像列表
        /// </summary>
        [Argument(Name = "对像列表", ControlType = ControlType.Variable, Order = 7, Description = "")]
        public List<string> VarNameList { get; set; } = new List<string>();

        [Argument(Name = "保存_id至字符变量", ControlType = ControlType.CheckBox, Order = 9, Description = "插入记录成功以后，将_id值存入字符变量")]
        public bool SaveInsertId { get; set; }

        [Argument(Name = "_id存入", ControlType = ControlType.Variable, Order = 13, Description = "插入记录成功以后，将_id值存入变量")]
        public string InsertIdVar { get; set; }

        /// <summary>
        /// 查询的保存结果和插入的记录id
        /// </summary>
        [Argument(Name = "保存记录", ControlType = ControlType.Variable, Order = 16, Description = "")]
        public string SelectVarName { get; set; }

        /// <summary>
        /// 用法 http://mongodb.github.io/mongo-csharp-driver/2.10/getting_started/quick_tour/
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(ActivityContext context)
        {
            string host = context.ReplaceVar(this.ServerConn);
            string dbName = context.ReplaceVar(this.DataBase);
            string strCollect = context.ReplaceVar(this.Collection);

            var client = new MongoClient(host);

            //"mongodb+srv://<username>:<password>@<cluster-address>/test?w=majority"
            var database = client.GetDatabase(dbName);
            var collection = database.GetCollection<BsonDocument>(strCollect);
            Dictionary<string, object> dic = new Dictionary<string, object>();
            foreach (string s in this.VarNameList)
            {
                dic.Add(s, context.GetStr(s));
            }
            BsonDocument doc = new BsonDocument(dic);
            switch (this.MongodbCmdType)
            {
                case MongodbCmdType.Insert:
                    collection.InsertOne(doc);
                    object obj = doc.GetValue("_id");
                    string str = obj == null ? "" : obj.ToString();

                    if (this.SaveInsertId)
                    {
                        context.SetVarStr(this.InsertIdVar, str);
                        context.WriteLog($"写入Mongodb成功并将_id值为{str}写入变量{this.InsertIdVar}");
                    }
                    else
                    {
                        context.WriteLog($"写入Mongodb成功并得到_id值为：{str}");
                    }
                    break;
                case MongodbCmdType.Delete:
                    collection.DeleteMany(doc);
                    context.WriteLog($"删除Mongodb记录成功");
                    break;
                case MongodbCmdType.Update:
                    //collection.UpdateOne(doc,);
                    context.WriteLog("开发中");
                    break;
                case MongodbCmdType.Select:
                    var list = collection.Find(doc).ToList();
                    if (context.ContainsStr(this.SelectVarName))
                    {
                        context.SetVarStr(this.SelectVarName, list.ToJson());
                        context.WriteLog($"Mongodb选择{list.Count}条记录并存入字符变量{this.SelectVarName}成功");
                    }
                    else if (context.ContainsList(this.SelectVarName))
                    {
                        List<string> ls = new List<string>();
                        foreach (BsonDocument b in list) ls.Add(b.ToJson());
                        context.SetVarList(this.SelectVarName, ls);
                        context.WriteLog($"Mongodb选择{list.Count}条记录并存入列表变量{this.SelectVarName}成功");
                    }
                    break;
            }

            //MongoCollection<BsonDocument> mydbTable = database.GetCollection<BsonDocument>("student");
        }

        public override void Validate(ActivityContext context)
        {
            if (!this.ServerConn.ToLower().StartsWith("mongodb")) throw new Exception("服务器地址为空或格式错误");
            if (string.IsNullOrEmpty(this.DataBase)) throw new Exception("数据库不能为空");
            if (this.MongodbCmdType != MongodbCmdType.Select && this.VarNameList.Count == 0) throw new Exception("非查询集合操作的对像不能为空");
            if (this.MongodbCmdType == MongodbCmdType.Select && string.IsNullOrEmpty(this.SelectVarName)) throw new Exception("保存变量名称不能为空");
        }

        public override ControlStyle GetControlStyle(string field)
        {
            ControlStyle style = new ControlStyle() { Variables = ControlStyle.GetVariables(true, false, true) };
            switch (field)
            {
                case "InsertIdVar":
                    style.Visible = this.SaveInsertId && this.MongodbCmdType == MongodbCmdType.Insert;
                    break;
                case "SaveInsertId":
                    style.Visible = this.MongodbCmdType == MongodbCmdType.Insert;
                    break;
                case "VarNameList":
                    style.Visible = this.MongodbCmdType == MongodbCmdType.Insert;
                    break;
                case "SelectVarName":
                    style.Visible = this.MongodbCmdType == MongodbCmdType.Select;
                    break;
            }

            return style;
        }
    }
}