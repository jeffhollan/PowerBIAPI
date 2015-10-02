using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using PowerBIAPI.Models;
using System.Text;

namespace PowerBIAPI.Controllers
{
    public class PowerBIController : ApiController
    {
        public static AuthenticationResult authorization;
        public AuthenticationController authHelper = new AuthenticationController();

        
        public async Task<HttpResponseMessage> CreateDataset([FromBody]PowerBIDataset datasetRequest)
        {
            bool datasetExists = false;
            await authHelper.CheckToken();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authorization.AccessToken);
                var getResult = await client.GetAsync("https://api.powerbi.com/v1.0/myorg/datasets");
                JArray values = (JArray)JObject.Parse((await getResult.Content.ReadAsStringAsync()))["value"];
                foreach(var obj in values)
                {
                    if ((string)obj["name"] == ConfigurationManager.AppSettings["dataset"])
                    {
                        datasetExists = true;
                        ConfigurationManager.AppSettings.Add("datasetId", (string)obj["id"]);
                    }
                }

                if (datasetExists)
                    return Request.CreateResponse(HttpStatusCode.Conflict, "Dataset already existed. If you need to change the structure of the data you will need to delete the dataset in Power BI.");

                var createResult = await client.PostAsync("https://api.powerbi.com/v1.0/myorg/datasets", new StringContent(JsonConvert.SerializeObject(datasetRequest), Encoding.UTF8, "application/json"));


                if (createResult.StatusCode == HttpStatusCode.Created)
                {
                    string id = (string)JObject.Parse((await createResult.Content.ReadAsStringAsync()))["id"];
                    ConfigurationManager.AppSettings.Add("datasetId", id);
                    return Request.CreateResponse(HttpStatusCode.Created, "Dataset created, authorized and ready to go. Dataset ID: " + id);
                    
                }

                else
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "There was an error creating the dataset: " + (await createResult.Content.ReadAsStringAsync()));
            }
        }


    }
}
