using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using BpmHelper;
using Newtonsoft.Json;
using OThinker.H3.BizBus;
using OThinker.Data.Database;
using System.Data.SqlClient;
using H3.Upgrade.ApiHelper;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace H3.Upgrade
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {


        }

        private void button1_Click(object sender, EventArgs e)
        {
            #region 组织架构

            //工作日历
            //            var sqlcalander = string.Format(@"SELECT
            //	*
            //FROM OT_WorkingCalendar");
            //            var dtCalendar = H3DBHelper.GetDataTable(sqlcalander);
            //            foreach (DataRow item in dtCalendar.Rows)
            //            {
            //                var calendar = new OThinker.H3.Calendar.WorkingCalendar()
            //                {
            //                     ObjectID=item["ObjectID"].ToString(),
            //                     DisplayName = item["DisplayName"].ToString(),
            //                      IsDefault=
            //                    ObjectID = item["ObjectID"].ToString(),

            //                };

            //            }
            //组织类型
            var sql1 = string.Format(@"SELECT
	[ObjectID]
   ,[Code]
   ,[DisplayName]
   ,[SegmentApplicable]
FROM [dbo].[OT_Category]");

            var dt1 = H3DBHelper.GetDataTable(sql1);
            foreach (DataRow item in dt1.Rows)
            {
                var c = new OThinker.Organization.OrgCategory
                {
                    ObjectID = item["ObjectID"].ToString(),
                    DisplayName = item["DisplayName"].ToString(),
                    Code = item["Code"].ToString()
                };
                if (!OThinker.H3.Controllers.AppUtility.Engine.Organization.ExistsOrgCategory(c.Code
                    ))
                {
                    OThinker.H3.Controllers.AppUtility.Engine.Organization.AddOrgCategory(c);

                }
            }
            //OU
            var RootID = "18f923a7-5a5e-426d-94ae-a55ad1a4b240";
            SyncUnit(RootID);

            //用户  



            var sqluser = string.Format(@"/****** Script for SelectTopNRows command from SSMS  ******/
SELECT 
	[ObjectID]
   ,[Birthday]
   ,[Gender]
   ,[EntryDate]
   ,[DepartureDate]
   ,[ServiceState]
   ,[Email]
   ,[Email2]
   ,[Appellation]
   ,[EmployeeNumber]
   ,[EmployeeRank]
   ,[Title]
   ,[Password]
   ,[Mobile]
   ,[ImageName]
   ,[WeChatAccount]
   ,[IsConsoleUser]
   ,[FacsimileTelephoneNumber]
   ,[IpPhone]
   ,[HomePhone]
   ,[OfficePhone]
   ,[Pager]
   ,[QQ]
   ,[RTX]
   ,[MSN]
   ,[Skype]
   ,[Country]
   ,[CountryName]
   ,[Province]
   ,[City]
   ,[Street]
   ,[PostOfficeBox]
   ,[PostalCode]
   ,[IDNumber]
   ,[SID]
   ,[MobileType]
   ,[DeviceToken]
   ,[Picture]
   ,[PrivacyLevel]
   ,[Notification]
   ,[NotifyType]
   ,[CalendarId]
   ,[DefaultLanguage]
   ,[MobileToken]
   ,[JPushID]
   ,[ParentID]
   ,[SourceParentID]
   ,[CompanyID]
   ,[Admin]
   ,[SystemID]
   ,[Visibility]
   ,[CategoryID]
   ,[State]
   ,[Name]
   ,[Description]
   ,[Code]
   ,[ManagerID]
   ,[ViceManagerID]
   ,[SecretaryID]
   ,[SupervisorID]
   ,[CreatedTime]
   ,[ModifiedTime]
   ,[Attachment]
   ,[SortKey]
   ,[WorkflowCode]
   ,[ParentObjectID]
   ,[ParentPropertyName]
   ,[ParentIndex]
FROM [dbo].[OT_User]
WHERE State = 0");
            var dtuser = H3DBHelper.GetDataTable(sqluser);
            foreach (DataRow item in dtuser.Rows)
            {
                try
                {
                    var user = new OThinker.Organization.User
                    {
                        ObjectID = item["ObjectID"].ToString(),
                        Code = item["Code"].ToString(),
                        Password = item["Password"].ToString(),
                        Appellation = item["Appellation"].ToString(),
                        EmployeeNumber = item["EmployeeNumber"].ToString(),
                        Email = item["Email"].ToString(),
                        Mobile = item["Mobile"].ToString(),
                        OfficePhone = item["OfficePhone"].ToString(),
                        WeChatAccount = item["WeChatAccount"].ToString(),
                        Name = item["Name"].ToString(),
                        ParentID = item["ParentID"].ToString(),
                        State = OThinker.Organization.State.Active,
                        ManagerID = item["ManagerID"].ToString(),
                        ServiceState = OThinker.Organization.UserServiceState.InService,
                        SortKey = int.Parse(item["SortKey"].ToString()),
                        EmployeeRank = int.Parse(item["EmployeeRank"].ToString()),
                        IsConsoleUser = item["IsConsoleUser"].ToString() == "0" ? false : true
                    };
                    if (OThinker.H3.Controllers.AppUtility.Engine.Organization.GetUnit(item["ObjectID"].ToString()) == null)
                    {
                        OThinker.H3.Controllers.AppUtility.Engine.Organization.AddUnit("", user);

                    }

                }
                catch (Exception ex)
                {

                    LogTextHelper.WriteLine(ex.Message);
                }
            }



            //角色 
            //说明V10版本中的角色相当于V9版本中职务、岗位、编制的集合。所以在同步时，要进行转化
            //岗位名称->角色名称
            //岗位编码->角色编码
            //岗位成员->角色用户
            //岗位所在部门->角色管理范围

            var sqlorgjob = string.Format(@"SELECT
	[ObjectID]
   ,[Code]
   ,[SuperiorCode]
   ,[DisplayName]
   ,[Description]
   ,[ParentObjectID]
   ,[ParentPropertyName]
   ,[ParentIndex]
   ,[Level]
FROM [dbo].[OT_OrgJob]
ORDER BY Code");
            var dtjob = H3DBHelper.GetDataTable(sqlorgjob);
            foreach (DataRow item in dtjob.Rows)
            {
                var orgpost = new OThinker.Organization.OrgPost
                {
                    ObjectID = item["ObjectID"].ToString(),
                    Code = item["Code"].ToString(),
                    Name = item["DisplayName"].ToString(),
                    JobLevel = item["Level"].ToString() == "" ? 0 : int.Parse(item["Level"].ToString())

                };

                var staff = string.Format(@"SELECT
	                                            t3.ChildID
                                               ,t2.ParentID
                                               ,t1.Code
                                            FROM OT_OrgJob t1
	                                            ,OT_OrgPost t2
	                                            ,OT_GroupChild t3
                                            WHERE t1.Code = t2.JobCode
                                            AND t2.ObjectID = t3.ParentObjectID
                                            AND t1.Code='{0}'
                                            ORDER BY t1.Code, t2.Code, t2.ParentID", orgpost.Code);

                var dtorgstaff = H3DBHelper.GetDataTable(staff);
                var list = new List<OThinker.Organization.OrgStaff>();
                foreach (DataRow item2 in dtorgstaff.Rows)
                {
                    var orgstaff = new OThinker.Organization.OrgStaff
                    {
                        OUScope = new string[] { item2["ParentID"].ToString() },
                        UserID = item2["ChildID"].ToString(),
                        ParentObjectID = orgpost.ObjectID

                    };
                    list.Add(orgstaff);
                }
                orgpost.ChildList = list.ToArray();
                OThinker.H3.Controllers.AppUtility.Engine.Organization.AddUnit("", orgpost);

            }


            //编制名称->角色名称
            //编制编码->角色编码
            //编制成员->角色用户
            //编制管理部门->角色管理范围

            //组
            var sqlgroup = string.Format(@"SELECT
	                                            ObjectID
                                               ,Name
                                               ,Code
                                               ,ParentID
                                            FROM [dbo].[OT_Group]");
            var dtgroup = H3DBHelper.GetDataTable(sqlgroup);
            foreach (DataRow item in dtgroup.Rows)
            {
                var group = new OThinker.Organization.Group()
                {
                    ObjectID = item["ObjectID"].ToString(),
                    Name = item["Name"].ToString(),
                    ParentID = item["ParentID"].ToString()
                };
                var groupchild = string.Format(@"SELECT
	                                                *
                                                FROM OT_GroupChild
                                                WHERE ParentObjectID = '{0}'", item["ObjectID"].ToString());
                var dtchild = H3DBHelper.GetDataTable(groupchild);
                foreach (DataRow item2 in dtchild.Rows)
                {
                    var list = new List<OThinker.Organization.GroupChild>();
                    var staff = new OThinker.Organization.GroupChild
                    {
                        ObjectID = item2["ObjectID"].ToString(),
                        ChildID = item2["ChildID"].ToString(),
                        ParentObjectID = item2["ParentObjectID"].ToString()
                    };
                    list.Add(staff);
                    group.ChildList = list.ToArray();

                }

                OThinker.H3.Controllers.AppUtility.Engine.Organization.AddUnit("", group);


            }
            #endregion

        }

        private void SyncUnit(string ParentID)
        {
            var sqlou = string.Format(@"
SELECT
	[ObjectID]
   ,[ChildType]
   ,[WeChatID]
   ,[CalendarId]
   ,[ParentID]
   ,[SourceParentID]
   ,[CompanyID]
   ,[Admin]
   ,[SystemID]
   ,[Visibility]
   ,[CategoryID]
   ,[State]
   ,[Name]
   ,[Description]
   ,[Code]
   ,[ManagerID]
   ,[ViceManagerID]
   ,[SecretaryID]
   ,[SupervisorID]
   ,[CreatedTime]
   ,[ModifiedTime]
   ,[Attachment]
   ,[SortKey]
   ,[WorkflowCode]
   ,[ParentObjectID]
   ,[ParentPropertyName]
   ,[ParentIndex]
FROM [dbo].[OT_OrganizationUnit]
WHERE ParentID = '{0}'", ParentID);

            var dtou = H3DBHelper.GetDataTable(sqlou);
            foreach (DataRow item in dtou.Rows)
            {
                var org = new OThinker.Organization.OrganizationUnit
                {
                    ObjectID = item["ObjectID"].ToString(),
                    WeChatID = int.Parse(item["WeChatID"].ToString()),
                    WorkflowCode = item["WorkflowCode"].ToString(),
                    CategoryCode = OThinker.H3.Controllers.AppUtility.Engine.Organization.GetCategoryByObjectID(item["CategoryID"].ToString()).Code,
                    IsRootUnit = false,
                    Name = item["Name"].ToString(),
                    Description = item["Description"].ToString(),
                    ParentID = item["ParentID"].ToString(),
                    ManagerID = item["ManagerID"].ToString(),
                    CreatedTime = DateTime.Now,
                    ModifiedTime = DateTime.Now,
                    SortKey = int.Parse(item["SortKey"].ToString()),
                    State = OThinker.Organization.State.Active,
                    Visibility = OThinker.Organization.VisibleType.Normal,
                    CalendarID = item["CalendarID"].ToString()
                };
                OThinker.H3.Controllers.AppUtility.Engine.Organization.AddUnit("", org);
                SyncUnit(org.ObjectID);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //业务连接池

            var sqlDbConn = string.Format(@"SELECT  [ObjectID]
      ,[DbCode]
      ,[DisplayName]
      ,[DbType]
      ,[DbConnectionString]
      ,[Description]
      ,[ParentObjectID]
      ,[ParentPropertyName]
      ,[ParentIndex]
  FROM ..[OT_BizDbConnectionConfig]");
            var dtou = H3DBHelper.GetDataTable(sqlDbConn);
            foreach (DataRow item in dtou.Rows)
            {
                OThinker.H3.Settings.BizDbConnectionConfig config = new OThinker.H3.Settings.BizDbConnectionConfig();
                config.ObjectID = item["ObjectID"].ToString();
                config.DbCode = item["DbCode"].ToString();
                config.DisplayName = item["DisplayName"].ToString();
                config.DbType = (OThinker.Data.Database.DatabaseType)Enum.Parse(typeof(OThinker.Data.Database.DatabaseType), item["DbType"].ToString());
                config.DbConnectionString = item["DbConnectionString"].ToString();
                config.Description = item["Description"].ToString();
                OThinker.H3.Controllers.AppUtility.Engine.SettingManager.AddBizDbConnectionConfig(config);
            }
            //SAP连接池
            //业务服务菜单节点
            SyncBizService("BizBus_BizService");
            AddService("BizBus_BizService");
        }

        private void SyncBizService(string FolderCode)
        {
            //生成菜单节点
            var sqlnode = string.Format(@"SELECT 
	[ObjectID]
   ,[Code]
   ,[IsSystem]
   ,[ParentCode]
   ,[DisplayName]
   ,[LockedBy]
   ,[Description]
   ,[SortKey]
   ,[NodeType]
   ,[OpenNewWindow]
   ,[IconType]
   ,[IconCss]
   ,[IconUrl]
   ,[Url]
   ,[ParentObjectID]
   ,[ParentPropertyName]
   ,[ParentIndex]
FROM..[OT_FunctionNode]
WHERE ParentCode = '{0}'", FolderCode);
            var dtnode = H3DBHelper.GetDataTable(sqlnode);
            foreach (DataRow itemnode in dtnode.Rows)
            {
                var funcnode = new OThinker.H3.Acl.FunctionNode()
                {
                    ObjectID = itemnode["ObjectID"].ToString(),
                    Code = itemnode["Code"].ToString(),
                    IsSystem = Convert.ToBoolean(int.Parse(itemnode["IsSystem"].ToString())),
                    ParentCode = itemnode["ParentCode"].ToString(),
                    DisplayName = itemnode["DisplayName"].ToString(),
                    Description = itemnode["Description"].ToString(),
                    SortKey = int.Parse(itemnode["SortKey"].ToString()),
                    NodeType = OThinker.H3.Acl.FunctionNodeType.RuleFolder,
                    OpenNewWindow = Convert.ToBoolean(int.Parse(itemnode["OpenNewWindow"].ToString())),
                    IconType = OThinker.H3.Acl.IconType.Url
                };
                var result = OThinker.H3.Controllers.AppUtility.Engine.FunctionAclManager.GetFunctionNode(funcnode.ObjectID);
                if (result == null)
                {
                    OThinker.H3.Controllers.AppUtility.Engine.FunctionAclManager.AddFunctionNode(funcnode);

                }
                else
                {
                    OThinker.H3.Controllers.AppUtility.Engine.FunctionAclManager.RemoveFunctionNode(funcnode.ObjectID, false);
                    OThinker.H3.Controllers.AppUtility.Engine.FunctionAclManager.AddFunctionNode(funcnode);

                }
                AddService(funcnode.ObjectID);

                //遍历查找子菜单节点
                var childsql = string.Format(@"SELECT
	Code
FROM OT_FunctionNode
WHERE ParentCode = '{0}'
ORDER BY SortKey", funcnode.ObjectID);
                var dtchild = H3DBHelper.GetDataTable(childsql);
                foreach (DataRow childitem in dtchild.Rows)
                {
                    SyncBizService(childitem["Code"].ToString());
                }
            }
        }


        private void AddService(string FolderCode)
        {
            var sqlFolder = string.Format(@"SELECT  [ObjectID]
      ,[BizAdapterCode]
      ,[Code]
      ,[FolderCode]
      ,[DisplayName]
      ,[Description]
      ,[EnableAccountMapping]
      ,[AccountCategory]
      ,[VersionNo]
      ,[AllowCustomMethods]
      ,[ModifiedTime]
      ,[UsedCount]
      ,[ParentObjectID]
      ,[ParentPropertyName]
      ,[ParentIndex]
  FROM ..[OT_BizService]
  WHERE FolderCode='{0}'", FolderCode);
            var dtservice = H3DBHelper.GetDataTable(sqlFolder);
            foreach (DataRow item in dtservice.Rows)
            {
                OThinker.H3.BizBus.BizService.BizService bizService = new OThinker.H3.BizBus.BizService.BizService()
                {
                    ObjectID = item["ObjectID"].ToString(),
                    BizAdapterCode = item["BizAdapterCode"].ToString(),
                    Code = item["Code"].ToString(),
                    FolderCode = item["FolderCode"].ToString(),
                    DisplayName = item["DisplayName"].ToString(),
                    Description = item["Description"].ToString(),
                    EnableAccountMapping = Convert.ToBoolean(int.Parse(item["EnableAccountMapping"].ToString())),
                    AccountCategory = item["AccountCategory"].ToString(),
                    VersionNo = int.Parse(item["VersionNo"].ToString()),
                    AllowCustomMethods = Convert.ToBoolean(int.Parse(item["AllowCustomMethods"].ToString())),
                    ModifiedTime = DateTime.Now,
                    UsedCount = int.Parse(item["UsedCount"].ToString())
                };
                //参数设置
                var sqlsetting = string.Format(@"SELECT  [ObjectID]
                                                      ,[SettingName]
                                                      ,[SettingValue]
                                                      ,[ParentObjectID]
                                                      ,[ParentPropertyName]
                                                      ,[ParentIndex]
                                                  FROM ..[OT_BizServiceSetting]
                                                  WHERE ParentObjectID='{0}'", item["ObjectID"].ToString());
                var dtsetting = H3DBHelper.GetDataTable(sqlsetting);

                foreach (DataRow itemsetting in dtsetting.Rows)
                {
                    var name = itemsetting["SettingName"].ToString();
                    var value = itemsetting["SettingValue"].ToString().Replace("<string />", "").Replace("<string>", "").Replace("</string>", "");
                    bizService.SetSettingValue(name, value);

                }

                //方法列表
                var sqlmethod = string.Format(@"SELECT 
	[ObjectID]
   ,[MethodName]
   ,[ReturnType]
   ,[DisplayName]
   ,[Description]
   ,[UsedCount]
   ,[MethodSetting]
   ,[ParentObjectID]
   ,[ParentPropertyName]
   ,[ParentIndex]
FROM ..[OT_BizServiceMethod]
WHERE ParentObjectID = '{0}'
ORDER BY ParentIndex", bizService.ObjectID);
                var dtmethod = H3DBHelper.GetDataTable(sqlmethod);
                foreach (DataRow meitem in dtmethod.Rows)
                {
                    var Method = new OThinker.H3.BizBus.BizService.BizServiceMethod()
                    {
                        ObjectID = meitem["ObjectID"].ToString(),
                        MethodName = meitem["MethodName"].ToString(),
                        DisplayName = meitem["DisplayName"].ToString(),
                        Description = meitem["Description"].ToString(),
                        UsedCount = int.Parse(meitem["UsedCount"].ToString()),
                        ReturnType = (OThinker.H3.BizBus.BizService.MethodReturnType)Enum.Parse(typeof(OThinker.H3.BizBus.BizService.MethodReturnType), meitem["ReturnType"].ToString()),
                        MethodSetting = System.Web.HttpUtility.UrlDecode(meitem["MethodSetting"].ToString(), System.Text.Encoding.UTF8)

                    };



                    var methodresult = bizService.AddMethod(Method);

                }
                OThinker.H3.Controllers.AppUtility.Engine.BizBus.RemoveBizService(bizService.Code);
                var result2 = OThinker.H3.Controllers.AppUtility.Engine.BizBus.AddBizService(bizService, true);

            }

        }


        private void button3_Click(object sender, EventArgs e)
        {
            SyncBizService("BizRule_ListRuleTable");

            //rule
            var sqlrule = string.Format(@"SELECT 
	[ObjectID]
   ,[RuleCode]
   ,[Content]
   ,[DisplayName]
   ,[Description]
   ,[CreatedTime]
   ,[ModifiedTime]
FROM..[OT_BizRule]");
            var dtrule = H3DBHelper.GetDataTable(sqlrule);

            foreach (DataRow item in dtrule.Rows)
            {
                var insertrule = string.Format(@"INSERT INTO [dbo].[OT_BizRule]
           ([ObjectID]
           ,[RuleCode]
           ,[Content]
           ,[DisplayName]
           ,[Description]
           ,[CreatedTime]
           ,[ModifiedTime])
     VALUES
           ('{0}'
           ,'{1}'
           ,'{2}'
           ,'{3}'
           ,'{4}'
           ,GETDATE()
           ,GETDATE())", item["ObjectID"].ToString(),
           item["RuleCode"].ToString(),
           item["Content"].ToString(),
           item["DisplayName"].ToString(),
           item["Description"].ToString());
                H3DBHelper.ExecuteNonQuery(insertrule);

            }

            //OThinker.H3.Controllers.AppUtility.Engine.BizRuleAclManager.
        }

        private void AddFunctionNode(string NodeCode)
        {

            //生成菜单节点
            var sqlnode = string.Format(@"SELECT 
	[ObjectID]
   ,[Code]
   ,[IsSystem]
   ,[ParentCode]
   ,[DisplayName]
   ,[LockedBy]
   ,[Description]
   ,[SortKey]
   ,[NodeType]
   ,[OpenNewWindow]
   ,[IconType]
   ,[IconCss]
   ,[IconUrl]
   ,[Url]
   ,[ParentObjectID]
   ,[ParentPropertyName]
   ,[ParentIndex]
FROM..[OT_FunctionNode]
WHERE ParentCode = '{0}'", NodeCode);
            var dtnode = H3DBHelper.GetDataTable(sqlnode);
            foreach (DataRow itemnode in dtnode.Rows)
            {
                var funcnode = new OThinker.H3.Acl.FunctionNode()
                {
                    ObjectID = itemnode["ObjectID"].ToString(),
                    Code = itemnode["Code"].ToString(),
                    IsSystem = Convert.ToBoolean(int.Parse(itemnode["IsSystem"].ToString())),
                    ParentCode = itemnode["ParentCode"].ToString(),
                    DisplayName = itemnode["DisplayName"].ToString(),
                    Description = itemnode["Description"].ToString(),
                    SortKey = int.Parse(itemnode["SortKey"].ToString()),
                    NodeType =
                    (OThinker.H3.Acl.FunctionNodeType)Enum.Parse(typeof(OThinker.H3.Acl.FunctionNodeType), itemnode["NodeType"].ToString()),
                    OpenNewWindow = Convert.ToBoolean(int.Parse(itemnode["OpenNewWindow"].ToString())),
                    IconType = OThinker.H3.Acl.IconType.Url
                };
                var result = OThinker.H3.Controllers.AppUtility.Engine.FunctionAclManager.GetFunctionNode(funcnode.ObjectID);
                if (result == null)
                {
                    OThinker.H3.Controllers.AppUtility.Engine.FunctionAclManager.AddFunctionNode(funcnode);

                }
                else
                {
                    OThinker.H3.Controllers.AppUtility.Engine.FunctionAclManager.RemoveFunctionNode(funcnode.ObjectID, false);
                    OThinker.H3.Controllers.AppUtility.Engine.FunctionAclManager.AddFunctionNode(funcnode);

                }

                AddFunctionNode(funcnode.ObjectID);

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            #region 数据模型保存

            var sqlDraft = string.Format(@"SELECT 
	[ObjectID]
   ,[SchemaCode]
   ,[Content]
   ,[DisplayName]
   ,[Description]
   ,[CreatedTime]
   ,[ModifiedTime]
   ,[ListenerPolicy]
   ,[ParentObjectID]
   ,[ParentPropertyName]
   ,[ParentIndex]
FROM..[OT_BizObjectSchemaDraft]");

            var dtDraft = H3DBHelper.GetDataTable(sqlDraft);
            foreach (DataRow item in dtDraft.Rows)
            {
                var insertdraft = string.Format(@"INSERT INTO [dbo].[OT_BizObjectSchemaDraft]
           ([ObjectID]
           ,[SchemaCode]
           ,[Content]
           ,[DisplayName]
           ,[Description]
           ,[CreatedTime]
           ,[ModifiedTime]
           ,[ListenerPolicy])
     VALUES
           ('{0}'
           ,'{1}'
           ,'{2}'
           ,'{3}'
           ,'{4}'
           ,'{5}'
           ,'{6}'
           ,'{7}')", item["ObjectID"].ToString(),
           item["SchemaCode"].ToString(),
           item["Content"].ToString(),
           item["DisplayName"].ToString(),
           item["Description"].ToString(),
           item["CreatedTime"].ToString(),
           item["ModifiedTime"].ToString(),
           item["ListenerPolicy"].ToString());
                H3DBHelper.ExecuteNonQuery(insertdraft);
            }
            #endregion

            #region 数据模型发布


            var sqlPublished = string.Format(@"SELECT [ObjectID]
      ,[SchemaState]
      ,[SchemaCode]
      ,[Content]
      ,[DisplayName]
      ,[Description]
      ,[CreatedTime]
      ,[ModifiedTime]
      ,[ListenerPolicy]
  FROM ..[OT_BizObjectSchemaPublished]");

            var dtPublished = H3DBHelper.GetDataTable(sqlPublished);
            foreach (DataRow item in dtPublished.Rows)
            {
                var insertPublished = string.Format(@"INSERT INTO [dbo].[OT_BizObjectSchemaPublished]
           ([ObjectID]
           ,[SchemaState]
           ,[SchemaCode]
           ,[Content]
           ,[DisplayName]
           ,[Description]
           ,[CreatedTime]
           ,[ModifiedTime]
           ,[ListenerPolicy])
     VALUES
           ('{0}'
           ,'{1}'
           ,'{2}'
           ,'{3}'
           ,'{4}'
           ,'{5}'
           ,'{6}'
           ,'{7}'
           ,'{8}')", item["ObjectID"].ToString(),
           item["SchemaState"].ToString(),
           item["SchemaCode"].ToString(),
           item["Content"].ToString(),
           item["DisplayName"].ToString(),
           item["Description"].ToString(),
           item["CreatedTime"].ToString(),
           item["ModifiedTime"].ToString(),
           item["ListenerPolicy"].ToString());
                H3DBHelper.ExecuteNonQuery(insertPublished);
            }
            #endregion

            #region 查询列表
            var sqlbizquery = string.Format(@"SELECT 
	[ObjectID]
   ,[SchemaCode]
   ,[QueryCode]
   ,[DisplayName]
   ,[ListMethod]
   ,[ListDefault]
FROM [OT_BizQuery]");

            var dtquery = H3DBHelper.GetDataTable(sqlbizquery);

            foreach (DataRow item in dtquery.Rows)
            {
                var insertquery = string.Format(@"INSERT INTO [dbo].[OT_BizQuery]
                                                           ([ObjectID]
                                                           ,[SchemaCode]
                                                           ,[QueryCode]
                                                           ,[DisplayName]
                                                           ,[ListMethod]
                                                           ,[ListDefault])
                                                     VALUES
                                                           ('{0}'
                                                           ,'{1}'
                                                           ,'{2}'
                                                           ,'{3}'
                                                           ,'{4}'
                                                           ,'{5}')",
                                                           item["ObjectID"].ToString(),
                                                           item["SchemaCode"].ToString(),
                                                           item["QueryCode"].ToString(),
                                                           item["DisplayName"].ToString(),
                                                           item["ListMethod"].ToString(),
                                                           item["ListDefault"].ToString());

                H3DBHelper.ExecuteNonQuery(insertquery);
            }

            var sqlaction = string.Format(@"SELECT 
	[ObjectID]
   ,[ActionCode]
   ,[DisplayName]
   ,[ActionType]
   ,[BizMethodName]
   ,[BizSheetCode]
   ,[Url]
   ,[Confirm]
   ,[AfterSave]
   ,[WithID]
   ,'1'
   ,'0'
   ,[SortKey]
   ,[Icon]
   ,[ParentObjectID]
   ,[ParentPropertyName]
   ,[ParentIndex]
FROM [OT_BizQueryAction]");

            var dtaction = H3DBHelper.GetDataTable(sqlaction);

            foreach (DataRow item in dtaction.Rows)
            {
                var insertquery = string.Format(@"INSERT INTO [dbo].[OT_BizQueryAction]
           ([ObjectID]
           ,[ActionCode]
           ,[DisplayName]
           ,[ActionType]
           ,[BizMethodName]
           ,[BizSheetCode]
           ,[Url]
           ,[Confirm]
           ,[AfterSave]
           ,[WithID]
           ,[Visible]
           ,[IsDefault]
           ,[SortKey]
           ,[Icon]
           ,[ParentObjectID]
           ,[ParentPropertyName]
           ,[ParentIndex])
     VALUES
                                                           ('{0}'
                                                           ,'{1}'
                                                           ,'{2}'
                                                           ,{3}
                                                           ,'{4}'
                                                           ,'{5}'
                                                           ,'{6}'
                                                           ,{7}
                                                           ,{8}
                                                           ,{9}
                                                           ,{10}
                                                           ,{11}
                                                           ,{12}
                                                           ,'{13}'
                                                           ,'{14}'
                                                           ,'{15}'
                                                           ,{16}
)",
                                           item["ObjectID"].ToString(),
                                           item["ActionCode"].ToString(),
                                           item["DisplayName"].ToString(),
                                           item["ActionType"].ToString(),
                                           item["BizMethodName"].ToString(),
                                           item["BizSheetCode"].ToString(),
                                           item["Url"].ToString(),
                                           item["Confirm"].ToString(),
                                           item["AfterSave"].ToString(),
                                           item["WithID"].ToString(),
                                           1,
                                           0,
                                           item["SortKey"].ToString(),
                                           item["Icon"].ToString(),
                                           item["ParentObjectID"].ToString(),
                                           item["ParentPropertyName"].ToString(),
                                           item["ParentIndex"].ToString()
                                           );

                H3DBHelper.ExecuteNonQuery(insertquery);

            }

            var sqlColumn = string.Format(@"SELECT 
	[ObjectID]
   ,[PropertyName]
   ,[Visible]
   ,[Sortable]
   ,[Width]
   ,[DisplayFormat]
  -- ,[Zindex]
   ,[ParentObjectID]
   ,[ParentPropertyName]
   ,[ParentIndex]
FROM [OT_BizQueryColumn]");

            var dtColumn = H3DBHelper.GetDataTable(sqlColumn);
            foreach (DataRow item in dtColumn.Rows)
            {
                var insertcolumn = string.Format(@"INSERT INTO [dbo].[OT_BizQueryColumn]
           ([ObjectID]
           ,[PropertyName]
           ,[Visible]
           ,[Sortable]
           ,[Width]
           ,[DisplayFormat]
           ,[Zindex]
           ,[ParentObjectID]
           ,[ParentPropertyName]
           ,[ParentIndex])
     VALUES
           ('{0}'
           ,'{1}'
           ,{2}
           ,{3}
           ,{4}
           ,'{5}'
           ,{6}
           ,'{7}'
           ,'{8}'
           ,{9})"
, item["ObjectID"].ToString()
, item["PropertyName"].ToString()
, item["Visible"].ToString()
, item["Sortable"].ToString()
, item["Width"].ToString()
, item["DisplayFormat"].ToString()
, 0
, item["ParentObjectID"].ToString()
, item["ParentPropertyName"].ToString()
, item["ParentIndex"].ToString()
);
                H3DBHelper.ExecuteNonQuery(insertcolumn);


            }

            var sqlitem = string.Format(@"SELECT 
	[ObjectID]
   ,[PropertyName]
   ,[Visible]
   ,[PropertyType]
   ,[DisplayType]
   ,[FilterType]
   ,[SelectedValues]
   ,[DefaultValue]
   ,[ParentObjectID]
   ,[ParentPropertyName]
   ,[ParentIndex]
FROM [OT_BizQueryItems]");
            var dtitem = H3DBHelper.GetDataTable(sqlitem);

            foreach (DataRow item in dtitem.Rows)
            {
                var insertitem = string.Format(@"INSERT INTO [dbo].[OT_BizQueryItems]
           ([ObjectID]
           ,[PropertyName]
           ,[Visible]
           ,[PropertyType]
           ,[DisplayType]
           ,[FilterType]
           ,[SelectedValues]
           ,[DefaultValue]
           ,[ParentObjectID]
           ,[ParentPropertyName]
           ,[ParentIndex])
     VALUES
           ('{0}'
           ,'{1}'
           ,{2}
           ,{3}
           ,{4}
           ,{5}
           ,'{6}'
           ,'{7}'
           ,'{8}'
           ,'{9}'
           ,{10})"
, item["ObjectID"].ToString()
, item["PropertyName"].ToString()
, item["Visible"].ToString()
, item["PropertyType"].ToString()
, item["DisplayType"].ToString()
, item["FilterType"].ToString()
, item["SelectedValues"].ToString()
, item["DefaultValue"].ToString()
, item["ParentObjectID"].ToString()
, item["ParentPropertyName"].ToString()
, item["ParentIndex"].ToString()

);

                H3DBHelper.ExecuteNonQuery(insertitem);
            }

            var sqlListener = string.Format(@"SELECT 
	[ObjectID]
   ,[SchemaCode]
   ,[BizObjectId]
   ,[Condition]
   ,[CreatedTime]
   ,[Message]
FROM [OT_BizListener]");

            var dtlistener = H3DBHelper.GetDataTable(sqlListener);

            foreach (DataRow item in dtlistener.Rows)
            {
                var insertlistener = string.Format(@"INSERT INTO [dbo].[OT_BizListener]
           ([ObjectID]
           ,[SchemaCode]
           ,[BizObjectId]
           ,[Condition]
           ,[CreatedTime]
           ,[Message])
     VALUES
           ('{0}'
           ,'{1}'
           ,'{2}'
           ,'{3}'
           ,'{4}'
           ,'{5}')"
, item["ObjectID"].ToString()
, item["SchemaCode"].ToString()
, item["BizObjectId"].ToString()
, item["Condition"].ToString()
, item["CreatedTime"].ToString()
, item["Message"].ToString()
);
                H3DBHelper.ExecuteNonQuery(insertlistener);
            }






            #endregion


        }


        private void button7_Click(object sender, EventArgs e)
        {
            //
            AddFunctionNode("ProcessModel_BizMasterData");
            //AddFunctionNode("ProcessModel");
            MessageBox.Show("OK");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var sqlsheet = string.Format(@"SELECT 
	[ObjectID]
   ,[BizObjectSchemaCode]
   ,[SheetCode]
   ,[DisplayName]
   ,[LastModifiedBy]
   ,[LastModifiedTime]
   ,[SheetType]
   ,[DesignModeContent]
   ,[PrintModel]
   ,[Javascript]
   ,[RuntimeContent]
   ,[DraftRuntimeContent]
   ,[EnabledCode]
    ,0 'IsShared'
    ,'' 'OwnSchemaCode'
   ,[CodeContent]
   ,[SheetAddress]
   ,[MobileSheetAddress]
   ,[IsMVC]
   ,[PrintSheetAddress]
FROM [OT_BizSheet]");

            var dtsheet = H3DBHelper.GetDataTable(sqlsheet);

            var parameters = new List<SqlParameter>();

            parameters.Add(new SqlParameter() { ParameterName = "@OT_BizSheet9", Value = dtsheet });//值为上面转换的datatable

            H3DBHelper.ExecuteProcNonQuery("Pro_OT_BizSheet", parameters);

        }

        private void button6_Click(object sender, EventArgs e)
        {
            //流程模板
            var sqlClause = string.Format(@"SELECT
dbo.OT_WorkflowClause.BizSchemaCode,
dbo.OT_WorkflowClause.CalendarId,
dbo.OT_WorkflowClause.CurrentNewVersion,
dbo.OT_WorkflowClause.DefaultVersion,
dbo.OT_WorkflowClause.DisplayName,
dbo.OT_WorkflowClause.ExceptionManager,
dbo.OT_WorkflowClause.Icon,
dbo.OT_WorkflowClause.IconFileName,
dbo.OT_WorkflowClause.MobileStart,
dbo.OT_WorkflowClause.ObjectID,
dbo.OT_WorkflowClause.SeqNoResetType,
dbo.OT_WorkflowClause.SequenceCode,
dbo.OT_WorkflowClause.SortKey,
dbo.OT_WorkflowClause.State,
dbo.OT_WorkflowClause.WorkflowCode 
FROM
	dbo.OT_WorkflowClause");

            var dtClause = H3DBHelper.GetDataTable(sqlClause);
            foreach (DataRow item in dtClause.Rows)
            {
                var insertClause = string.Format(@"INSERT INTO [dbo].[OT_WorkflowClause] ([ObjectID]
, [SortKey]
, [WorkflowName]
, [BizSchemaCode]
, [WorkflowCode]
, [CurrentNewVersion]
, [DefaultVersion]
, [State]
, [MobileStart]
, [SequenceCode]
, [SeqNoResetType]
, [IconFileName]
, [Icon]
, [CalendarId]
, [ExceptionManager])
	VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}')"
, item["ObjectID"].ToString()
, item["SortKey"].ToString()
, item["DisplayName"].ToString()
, item["BizSchemaCode"].ToString()
, item["WorkflowCode"].ToString()
, item["CurrentNewVersion"].ToString()
, item["DefaultVersion"].ToString()
, item["State"].ToString()
, item["MobileStart"].ToString()
, item["SequenceCode"].ToString()
, item["SeqNoResetType"].ToString()
, item["IconFileName"].ToString()
, item["Icon"].ToString()
, item["CalendarId"].ToString()
, item["ExceptionManager"].ToString()
);
                H3DBHelper.ExecuteNonQuery(insertClause);
            }

            //流程流水号
            var sqlSeqNo = string.Format(@"SELECT
	dbo.OT_WorkflowClauseSeqNo.NextInstanceSeqID,
	dbo.OT_WorkflowClauseSeqNo.ObjectID,
	dbo.OT_WorkflowClauseSeqNo.WorkflowCode 
FROM
	dbo.OT_WorkflowClauseSeqNo");

            var dtSeqNo = H3DBHelper.GetDataTable(sqlSeqNo);
            foreach (DataRow item in dtSeqNo.Rows)
            {
                var insertseq = string.Format(@"INSERT INTO [dbo].[OT_WorkflowClauseSeqNo]
           ([ObjectID]
           ,[WorkflowCode]
           ,[NextInstanceSeqID])
    VALUES ('{0}', '{1}', {2})"
, item["ObjectID"].ToString()
, item["WorkflowCode"].ToString()
, item["NextInstanceSeqID"].ToString()
    );
                H3DBHelper.ExecuteNonQuery(insertseq);
            }

            //流程图保存版本
            var sqlDraft = string.Format(@"SELECT 
	[ObjectID]
   ,[Content]
   ,[Creator]
   ,[ModifiedBy]
   ,[CreatedTime]
   ,[ModifiedTime]
   ,[WorkflowCode]
   ,[BizObjectSchemaCode]
FROM 
dbo.OT_WorkflowTemplateDraft");

            var dtDraft = H3DBHelper.GetDataTable(sqlDraft);
            if (dtDraft.Rows.Count > 0)
            {
                var parameters = new List<SqlParameter>();

                parameters.Add(new SqlParameter() { ParameterName = "@TemplateDraft9", Value = dtDraft });//值为上面转换的datatable

                H3DBHelper.ExecuteProcNonQuery("Pro_WorkflowTemplateDraft", parameters);

            }


            //流程模板发布版本
            var sqlPublished = string.Format(@"SELECT
dbo.OT_WorkflowTemplatePublished.BizObjectSchemaCode,
dbo.OT_WorkflowTemplatePublished.Content,
dbo.OT_WorkflowTemplatePublished.ObjectID,
dbo.OT_WorkflowTemplatePublished.ParentIndex,
dbo.OT_WorkflowTemplatePublished.ParentObjectID,
dbo.OT_WorkflowTemplatePublished.ParentPropertyName,
dbo.OT_WorkflowTemplatePublished.Publisher,
dbo.OT_WorkflowTemplatePublished.PublishTime,
dbo.OT_WorkflowTemplatePublished.StartActivityCode,
dbo.OT_WorkflowTemplatePublished.WorkflowCode,
dbo.OT_WorkflowTemplatePublished.WorkflowVersion

FROM
dbo.OT_WorkflowTemplatePublished
");
            var dtPublished = H3DBHelper.GetDataTable(sqlPublished);
            foreach (DataRow item in dtPublished.Rows)
            {
                var insertPublished = string.Format(@"INSERT INTO [dbo].[OT_WorkflowTemplatePublished] ([ObjectID]
, [Content]
, [StartActivityCode]
, [WorkflowVersion]
, [Publisher]
, [PublishTime]
, [WorkflowCode]
, [BizObjectSchemaCode])
	VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')"
, item["ObjectID"].ToString()
, item["Content"].ToString()
, item["StartActivityCode"].ToString()
, item["WorkflowVersion"].ToString()
, item["Publisher"].ToString()
, item["PublishTime"].ToString()
, item["WorkflowCode"].ToString()
, item["BizObjectSchemaCode"].ToString()

);
                H3DBHelper.ExecuteNonQuery(insertPublished);
            }


            //流程发起权限
            var sqlacl = string.Format(@"SELECT
dbo.OT_WorkflowAcl.Administrator,
dbo.OT_WorkflowAcl.CreatedBy,
dbo.OT_WorkflowAcl.CreatedTime,
dbo.OT_WorkflowAcl.CreateInstance,
dbo.OT_WorkflowAcl.ModifiedBy,
dbo.OT_WorkflowAcl.ModifiedTime,
dbo.OT_WorkflowAcl.ObjectID,
dbo.OT_WorkflowAcl.ParentIndex,
dbo.OT_WorkflowAcl.ParentObjectID,
dbo.OT_WorkflowAcl.ParentPropertyName,
dbo.OT_WorkflowAcl.UserID,
dbo.OT_WorkflowAcl.WorkflowCode

FROM
dbo.OT_WorkflowAcl
");
            var dtacl = H3DBHelper.GetDataTable(sqlacl);
            foreach (DataRow item in dtacl.Rows)
            {
                var insetacl = string.Format(@"INSERT INTO [dbo].[OT_WorkflowAcl] ([ObjectID]
, [WorkflowCode]
, [CreateInstance]
, [UserID]
, [Administrator]
, [CreatedTime]
, [ModifiedTime]
, [CreatedBy]
, [ModifiedBy])
	VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{6}')"
, item["ObjectID"].ToString()
, item["WorkflowCode"].ToString()
, item["CreateInstance"].ToString()
, item["UserID"].ToString()
, item["Administrator"].ToString()
, item["CreatedTime"].ToString()
, item["ModifiedTime"].ToString()
, item["CreatedBy"].ToString()
, item["ModifiedBy"].ToString()

);
                H3DBHelper.ExecuteNonQuery(insetacl);
            }

        }

        private void button8_Click(object sender, EventArgs e)
        {
            //OT_InstanceContext
            SyncSystemTable("OT_InstanceContext");

            //OT_WorkItem
            SyncSystemTable("OT_WorkItem");

            SyncSystemTable("OT_Comment");

            SyncSystemTable("OT_Timer");

            SyncSystemTable("OT_Token");

            SyncSystemTable("OT_Urgency");
        }

        private string GetTableSql(string TableName)
        {
            var stringCol = H3DBHelper.GetTableColumns(TableName);
            return string.Format(@"select {0} 
from 
{1}", stringCol, TableName);
        }

        private void SyncSystemTable(string TableName)
        {
            try
            {
                var ProcName = TableName.Replace("OT_", "Proc_");
                var sql = GetTableSql(TableName);
                var dt = H3DBHelper.GetDataTable(sql);
                var parameters = new List<SqlParameter>();

                parameters.Add(new SqlParameter() { ParameterName = "@TempTable", Value = dt });//值为上面转换的datatable

                var result = H3DBHelper.ExecuteProcNonQuery(ProcName, parameters);
                //return result.ToString();
            }
            catch (Exception ex)
            {
                LogTextHelper.WriteLine("导入报错:" + TableName);
            }
        }

        private string SyncCustomerTable(string TableName)
        {
            try
            {
                var ProcName = "Proc_" + TableName;
                var sql = GetTableSql(TableName);
                var dt = H3DBHelper.GetDataTable(sql);
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter() { ParameterName = "@TempTable", Value = dt });//值为上面转换的datatable
                var result = H3DBHelper.ExecuteProcNonQuery(ProcName, parameters);
                LogTextHelper.WriteLine("导入表数据：" + TableName + ",导入数据条数：" + result.ToString());
                return result.ToString();
            }
            catch (Exception ex)
            {
                LogTextHelper.WriteLine("导入报错:" + TableName);
                LogTextHelper.WriteLine(ex.Message);
            }
            return "true";
        }

        /// <summary>
        /// 创建OT_开头的系统表的脚本
        /// </summary>
        private void CreateSystemTable()
        {
            var sqltable = string.Format(@"SELECT
	* 
FROM
	INFORMATION_SCHEMA.TABLES 
WHERE
	Table_Name LIKE 'OT_%'");
            var dt = OThinker.H3.Controllers.AppUtility.Engine.Query.QueryTable(sqltable);
            foreach (DataRow item in dt.Rows)
            {
                //创建Type
                var TableName = item["TABLE_NAME"].ToString();
                var TypeName = TableName.Replace("OT_", "Type_");
                var ProcName = TableName.Replace("OT_", "Proc_");
                var TypeCols = new List<string>();
                var ProCols = new List<string>();
                var Schema10 = H3DBHelper.GetTableSchema(TableName, "V10");
                var Schema9 = H3DBHelper.GetTableSchema(TableName, "V9");
                foreach (var filed in Schema10)
                {
                    var tt = Schema9.Where(a => a.Name == filed.Name);
                    if (Schema9.Contains(filed))
                    {
                        ProCols.Add(filed.Name);
                        if (filed.Length != null)
                        {
                            TypeCols.Add(string.Format(@"{0} [{1}] ({2}) NULL"
    , filed.Name
    , filed.Type
    , filed.Length));

                        }
                        else
                        {
                            TypeCols.Add(string.Format(@"{0} [{1}]  NULL"
, filed.Name
, filed.Type));

                        }

                    }
                }

                if (ProCols.Count > 0)
                {
                    var DropPeoc = string.Format(@"DROP PROCEDURE {0}", ProcName);

                    var DropType = string.Format(@"DROP TYPE {0}", TypeName);

                    H3DBHelper.ExecuteNonQuery(DropPeoc);
                    H3DBHelper.ExecuteNonQuery(DropType);


                    var createtype = string.Format(@"CREATE TYPE {0} AS TABLE({1})"
    , TypeName
    , string.Join(",\n", TypeCols.ToArray()));

                    H3DBHelper.ExecuteNonQuery(createtype);

                    //创建存储过程
                    var ColsJoin = string.Join(",\n", ProCols.ToArray());
                    var createproc = string.Format(@"CREATE PROCEDURE {0}
(
    @TempTable {1} Readonly 
)
AS
BEGIN
    SET NOCOUNT ON
    BEGIN TRANSACTION
INSERT INTO {2}
           ({3})
        SELECT   
           {3}
              FROM @TempTable
    COMMIT TRANSACTION           
END"
    , ProcName
    , TypeName
    , item["TABLE_NAME"].ToString()
    , ColsJoin);
                    H3DBHelper.ExecuteNonQuery(createproc);

                }
                else
                {
                    LogHelper.Warn("V9中不存在表：" + TableName);
                }
            }


        }

        private void CreateCustomerTable()
        {
            var sqltable = string.Format(@"SELECT
	*
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
AND Table_Name  LIKE 'I_%'");

            var dt = H3DBHelper.GetDataTable(sqltable);
            foreach (DataRow item in dt.Rows)
            {
                var TableName = item["TABLE_NAME"].ToString();


                //创建Type
                var TypeName = "Type_" + TableName;
                var ProcName = "Proc_" + TableName;
                var TypeCols = new List<string>();
                var ProCols = new List<string>();
                var Schema9 = H3DBHelper.GetTableSchema(TableName, "V9");
                foreach (var filed in Schema9)
                {

                    ProCols.Add("[" + filed.Name + "]");
                    if (filed.Length != null)
                    {
                        if (filed.Name.ToLower() == "objectid")
                        {
                            TypeCols.Add(string.Format(@"[{0}] [{1}] ({2}) NOT NULL"
    , filed.Name
    , filed.Type
    , filed.Length));
                        }
                        else
                        {
                            TypeCols.Add(string.Format(@"[{0}] [{1}] ({2}) NULL"
    , filed.Name
    , filed.Type
    , filed.Length));
                        }

                    }
                    else
                    {
                        TypeCols.Add(string.Format(@"[{0}] [{1}]  NULL"
, filed.Name
, filed.Type));

                    }


                }

                //在10版本中创建自定义Table
                var DropTable = string.Format(@"DROP Table {0}", TableName);
                H3DBHelper.ExecuteNonQuery(DropTable);

                //                var CreateTable = string.Format(@"CREATE TABLE {0}(
                //{1}
                //PRIMARY KEY CLUSTERED 
                //(
                //	[ObjectID] ASC
                //)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                //) ON [PRIMARY]
                //GO"
                //, TableName
                //, string.Join(",\n", TypeCols.ToArray()));
                var CreateTable = string.Format(@"CREATE TABLE {0}
(
{1}
)"
, TableName
, string.Join(",\n", TypeCols.ToArray()));

                H3DBHelper.ExecuteNonQuery(CreateTable);

                if (ProCols.Count > 0)
                {
                    //var DropPeoc = string.Format(@"DROP PROCEDURE {0}", ProcName);

                    //var DropType = string.Format(@"DROP TYPE {0}", TypeName);

                    //H3DBHelper.ExecuteNonQuery(DropPeoc);
                    //H3DBHelper.ExecuteNonQuery(DropType);


                    var createtype = string.Format(@"CREATE TYPE {0} AS TABLE({1})"
    , TypeName
    , string.Join(",\n", TypeCols.ToArray()));

                    H3DBHelper.ExecuteNonQuery(createtype);

                    //创建存储过程
                    var ColsJoin = string.Join(",\n", ProCols.ToArray());
                    var createproc = string.Format(@"CREATE PROCEDURE {0}
(
    @TempTable {1} Readonly 
)
AS
BEGIN
    SET NOCOUNT ON
    BEGIN TRANSACTION
INSERT INTO {2}
           ({3})
        SELECT   
           {3}
              FROM @TempTable
    COMMIT TRANSACTION           
END"
    , ProcName
    , TypeName
    , item["TABLE_NAME"].ToString()
    , ColsJoin);
                    H3DBHelper.ExecuteNonQuery(createproc);

                }

            }

        }
        /// <summary>
        /// 生成数据库脚本OT_存储过程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button12_Click(object sender, EventArgs e)
        {
            //CreateSystemTable();
            CreateCustomerTable();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            SyncCustomerTable("I_StockExchange");
            var sqltable = string.Format(@"SELECT
	*
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
AND Table_Name  LIKE 'I_%'");

            var dt = H3DBHelper.GetDataTable(sqltable);
            foreach (DataRow item in dt.Rows)
            {
                var TableName = item["TABLE_NAME"].ToString();
                SyncCustomerTable(TableName);
            }

        }

        private void button10_Click(object sender, EventArgs e)
        {
            SyncSystemTable("OT_FunctionNode");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            //系统管理员
            SyncSystemTable("OT_SystemAcl");
            //组织权限
            SyncSystemTable("OT_SystemOrgAcl");
            //文件储存
            SyncSystemTable("OT_FileServer");
            //工作日历
            SyncSystemTable("OT_WorkingCalendar");
            SyncSystemTable("OT_WorkingDay");
            SyncSystemTable("OT_WorkingTimeSpan");

            //数据字典
            SyncSystemTable("OT_EnumerableMetadata");

            //委托设置
            SyncSystemTable("OT_Agency");

        }

        private void button9_Click(object sender, EventArgs e)
        {
            LogHelper.Trace("Trace Message");
            LogHelper.Debug("Debug Message");
            LogHelper.Info("Info Message");
            LogHelper.Error("Error Message");
            LogHelper.Fatal("Fatal Message");
        }
    }
}
