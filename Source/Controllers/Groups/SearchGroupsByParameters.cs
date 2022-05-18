using System.Net;
using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using PoliFemoBackend.Source.Utils;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using JObject = Newtonsoft.Json.Linq.JObject;
using JArray = Newtonsoft.Json.Linq.JArray;
using System.Web;
using JSConverter = Newtonsoft.Json.JsonConverter; 




namespace PoliFemoBackend.Source.Controllers.Rooms;

[ApiController]
[Route("[controller]")]
public class SearchGroupsByParameters : ControllerBase
{
    [HttpGet]
    [HttpPost]
    public async Task<ObjectResult> SearchGroupByParameters(string name, string? year, string? degree, string? type, string? platform, string? language, string? office)
    {
        //get content from url
        var content = await Utils.HtmlUtil.DownloadHtmlAsync("https://raw.githubusercontent.com/PoliNetworkOrg/polinetworkWebsiteData/main/groups.json");

        if (content == null)
        {
            return new ObjectResult(new { error = "Errore durante il recupero dei gruppi" }) {StatusCode = (int) HttpStatusCode.InternalServerError};
        }

        var doc = new HtmlDocument();
      
        var c = content.GetData();
        if (c == null)
        {
            return new ObjectResult(new { error = "Errore durante il recupero dei gruppi" }) {StatusCode = (int) HttpStatusCode.InternalServerError};
        }
        {
            var c1 = c.Replace("<", "&lt;");
            doc.LoadHtml(c1); 
        }
        
        //WriteLine doc
        //Console.WriteLine(doc.DocumentNode.InnerHtml);  //tenere non cancellare


        //convert doc to json
        var json = JsonConvert.DeserializeObject<dynamic>(doc.DocumentNode.InnerHtml);

        //print json file
        //return Ok(json);

        //crea  json vuoto
        var results = new JObject();
        
        //crea lista results dentro json
        
        var resultsList = new JArray();
        

        

        //cicla json
        if (json == null)
        {
            return new ObjectResult(new { error = "Errore durante il recupero dei gruppi" }) {StatusCode = (int) HttpStatusCode.InternalServerError};
        }
        {
            foreach (var item in json.index_data)
            {
                //controlla se il gruppo ha il nome richiesto
                if (item["class"].ToString().ToLower().Contains(name.ToLower())){
                    //controlla se year è uguale a quello richiesto, in caso year non sia specificato controlla tutti i gruppi
                    if (year == null || item.year.ToString().ToLower().Contains(year.ToLower())){
                        //controlla se il gruppo ha il tipo richiesto
                        if (type == null || item.type.ToString().ToLower().Contains(type.ToString().ToLower())){
                            //controlla se il gruppo ha il livello di laurea richiesto
                            if (degree == null || item.degree.ToString().ToLower().Contains(degree.ToLower())){
                                //controlla se il gruppo ha la piattaforma richiesta
                                if (platform == null || item.platform.ToString().ToLower().Contains(platform.ToLower())){
                                    //controlla se il gruppo ha la lingua richiesta
                                    if (language == null || item.language.ToString().ToLower().Contains(language.ToLower())){
                                        //controlla se il gruppo ha l'ufficio richiesto
                                        if (office == null || item.office.ToString().ToLower().Contains(office.ToLower())){
                                            //aggiungi risultato alla lista
                                            resultsList.Add(JObject.Parse(HttpUtility.HtmlDecode(item.ToString())));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                    
            } 
        }
        
        results["groups"] = resultsList;
        
        //se la lista è vuota
        if (results.Count == 0)
        {
            return new ObjectResult(new { error = "Nessun gruppo trovato" }) {
                StatusCode = (int) HttpStatusCode.InternalServerError
            };
        }
        //se la lista contiene almeno un elemento
        else
        {
            return Ok(results);
        }
    }


}

