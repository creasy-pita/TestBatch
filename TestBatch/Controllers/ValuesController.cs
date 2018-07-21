using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enity;
using Log;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace TestBatch.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            Service service = new Service();
            return new string[] { service.ShowProject(), "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            string result = null;
            Projects dataSourceDesc = new Projects();
            Service service = new Service();
            try
            {
                switch (id)
                {
                    case 0:
                        dataSourceDesc.name = Request.Query["plan_name"];
                        dataSourceDesc.inner_source = Request.Query["from_datasource"];
                        dataSourceDesc.inner_name = Request.Query["from_username"];
                        dataSourceDesc.inner_pwd = Request.Query["from_userpassword"];
                        dataSourceDesc.out_source = Request.Query["target_datasource"];
                        dataSourceDesc.out_name = Request.Query["target_username"];
                        dataSourceDesc.out_pwd = Request.Query["target_userpassword"];
                        dataSourceDesc.synchron_cycle = Request.Query["circle"];
                        if (service.IsExist(dataSourceDesc))
                        {
                            result = service.SaveXML(dataSourceDesc);
                        }
                        else
                        {
                            result = "false";
                        }
                        break;
                    case 1:
                        if (service.ManulaSyc())
                        {
                            result = "批量同步操作成功！";
                        }
                        else
                        {
                            result = "批量同步失败，请查看日志！";
                        }
                        break;
                    case 2:
                        result = "自动同步开始！";
                        service.AutoSyc();
                        break;
                    case 3:
                        result = service.StopAutoSyc();
                        break;
                    case 4:
                        result = service.Open_log();
                        break;
                    case 5:
                        dataSourceDesc.name = Request.Query["plan_name"];
                        dataSourceDesc.inner_source = Request.Query["from_datasource"];
                        dataSourceDesc.inner_name = Request.Query["from_username"];
                        dataSourceDesc.inner_pwd = Request.Query["from_userpassword"];
                        dataSourceDesc.out_source = Request.Query["target_datasource"];
                        dataSourceDesc.out_name = Request.Query["target_username"];
                        dataSourceDesc.out_pwd = Request.Query["target_userpassword"];
                        dataSourceDesc.synchron_cycle = Request.Query["circle"];
                        result = service.UpdateXML(dataSourceDesc);
                        break;
                    case 6:
                        string ProjectName = Request.Query["plan_name"];
                        result = service.DeleteXML(ProjectName);
                        break;
                    case 7:
                        result = service.Find_AutoBackup_Symbol().ToString();
                        break;
                    case 8:
                        result = service.ShowProject();
                        break;
                    case 9:
                        result = service.ShowProjectParameter(Request.Query["plan_name"]);
                        break;
                    default:
                        string var = Request.Form["object"];
                        break;
                }
            }
            catch(Exception e)
            {
                WriteLog log = new WriteLog();
                if(id==2)//出现异常，先将数据库连接关闭,继续开启自动同步
                {
                    service.StopAutoSyc();
                    log.Logger("自动同步关闭");
                    Service s = new Service();
                    log.Logger("自动同步开启");
                    s.AutoSyc();
                }
                log.Logger(e.Message);
            }
            return result;
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
