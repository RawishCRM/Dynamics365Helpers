using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Crm.Sdk.Messages;
using System.Configuration;
using System.Net;
using System.ServiceModel.Description;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml;

namespace Dynamics
{
    class Program
    {
        static void Main(string[] args)
        {
           
            var fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='account'>
    <attribute name='name' />
    <attribute name='primarycontactid' />
    <attribute name='telephone1' />
    <attribute name='accountid' />
    <order attribute='name' descending='false' />
  </entity>
</fetch>";
            //execute using web api

            try
            {
               var redirectUrl = "ORG WEB API URL";

                HttpClient httpClient = null;
                httpClient = new HttpClient();
                //Default Request Headers needed to be added in the HttpClient Object
                httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Set the Authorization header with the Access Token received specifying the Credentials
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "Add bearer token");

                httpClient.BaseAddress = new Uri(redirectUrl);      


                var response = httpClient.GetAsync("accounts?fetchXml="+fetch+"").Result;
                if (response.IsSuccessStatusCode)
                {

                    var accounts = response.Content.ReadAsStringAsync().Result;
                   
                }
            }
            catch(Exception e)
            {
                throw new Exception();
            }


            // Organization service, execute same fetch
            IOrganizationService organizationService = null;

            ClientCredentials clientCredentials = new ClientCredentials();
                clientCredentials.UserName.UserName = ConfigurationManager.AppSettings["Username"];
                clientCredentials.UserName.Password = ConfigurationManager.AppSettings["Password"];

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                organizationService = (IOrganizationService)new OrganizationServiceProxy(new Uri(ConfigurationManager.AppSettings["CRMUrl"]),
                 null, clientCredentials, null);

                if (organizationService != null)
                {
                    Guid userid = ((WhoAmIResponse)organizationService.Execute(new WhoAmIRequest())).UserId;



                    
                   

                    try
                    {
                        var moreRecords = false;
                        int page = 1;
                        var cookie = string.Empty;
                        List<Entity> Entities = new List<Entity>();
                        do
                        {
                            var xml1 = fetch.Replace("##pagenumber##", page.ToString());
                            var xml = string.Format(xml1, cookie);

                            var collection = organizationService.RetrieveMultiple(new FetchExpression(xml));

                            //trace.Trace(collection.TotalRecordCount.ToString());
                            if (collection.Entities.Count >= 0) Entities.AddRange(collection.Entities);

                            moreRecords = collection.MoreRecords;
                            if (moreRecords)
                            {
                                ++page;
                                cookie = string.Format("paging-cookie='{0}' page='{1}'", System.Security.SecurityElement.Escape(collection.PagingCookie), page);
                            }
                        } while (moreRecords);

                        
                    }
                    catch (Exception e)
                    {
                        // Handle the exception.
                        throw e;
                    }
                }
                else
                {
                    Console.WriteLine("Failed to Established Connection!!!");
                }
            }
           
        }

    }
    

