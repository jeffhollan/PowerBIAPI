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
using TRex.Metadata;

namespace PowerBIAPI.Controllers
{
    public class PowerBIController : ApiController
    {
        public static AuthResult authorization;
        public AuthenticationController authHelper = new AuthenticationController();

        
        [HttpPost, Route("api/CreateDataset")]
        [Metadata(Visibility = VisibilityType.Internal)]
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

        [HttpPost, Route("api/AddRows")]
        [Metadata("Add Rows", "Add rows to a Power BI dataset")]
        [Swashbuckle.Swagger.Annotations.SwaggerResponse(HttpStatusCode.OK, "Added rows", typeof(PowerBIRows))]
        public async Task<HttpResponseMessage> AddRows([FromUri][Metadata("Dataset ID", "The Dataset ID from Power BI", VisibilityType.Default)] string datasetId, [FromBody][Metadata("Rows", "Comma-separated list of JSON Objects for each row to be inputted")] string rows)
        {
            PowerBIRows pbiRows = ConvertRowStringToPowerBIRows(rows);
            await authHelper.CheckToken();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authorization.AccessToken);
                var result = await client.PostAsync(string.Format("https://api.powerbi.com/v1.0/myorg/datasets/{0}/tables/Product/rows", datasetId), new StringContent(JsonConvert.SerializeObject(pbiRows), Encoding.UTF8, "application/json"));
                if (result.StatusCode == HttpStatusCode.OK)
                    return Request.CreateResponse<PowerBIRows>(HttpStatusCode.OK, pbiRows);
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        private PowerBIRows ConvertRowStringToPowerBIRows(string rows)
        {
            string arrayRows = "[ " + rows + " ]";
            JArray rowsArray = JArray.Parse(arrayRows);
            return new PowerBIRows { rows = rowsArray };            
        }
    }
}
