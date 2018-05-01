using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace ITCrewe
{
    public class Function
    {
        public dynamic FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            SkillResponse response;

            var requestType = input.GetRequestType();

            if (requestType == typeof(IntentRequest))
            {
                var request = input.Request as IntentRequest;
                response = GetResponse(request);
            }
            else
            {
                response = MakeSkillResponse("Welcome to ChuckNorries Facts", false, "Chuck Norris, waits for no man!");
            }

            return response;
        }

        protected SkillResponse MakeSkillResponse(string outputSpeech, bool endSession, string repromptText)
        {
            var response = new ResponseBody
            {
                ShouldEndSession = endSession,
                OutputSpeech = new PlainTextOutputSpeech { Text = outputSpeech }
            };

            if (repromptText != null)
            {
                response.Reprompt = new Reprompt() { OutputSpeech = new PlainTextOutputSpeech { Text = repromptText } };
            }

            var skillResponse = new SkillResponse
            {
                Response = response,
                Version = "1.0"
            };

            return skillResponse;
        }

        public SkillResponse GetResponse(IntentRequest request)
        {
            SkillResponse skillResponse;

            switch (request.Intent.Name)
            {
                case "fact":
                    //get fact
                    var fact = GetFacts().First();
                    skillResponse = MakeSkillResponse(fact, false, "Chuck Norris, waits for no man!");

                    break;
                case "multifact":
                    var count = int.Parse(request.Intent.Slots["count"].Value);

                   //get facts
                   var facts = GetFacts(count);

                    var strBuilder = new StringBuilder();

                    for (var i = 0; i < count; i++)
                    {
                        strBuilder.AppendLine($"Fact {i + 1}: " + facts[i]);
                    }

                    skillResponse = MakeSkillResponse(strBuilder.ToString(), false, "Chuck Norris, waits for no man!");

                    break;
                default:
                    skillResponse = MakeSkillResponse("Check Norris doesn't know what you want, so he left", true, "Chuck Norris, waits for no man!");
                    break;
            }

            return skillResponse;
        }

        public List<string> GetFacts(int count = 1)
        {
            var facts = new List<string>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.chucknorris.io/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                for (var i = 0; i < count; i++)
                {
                    var response = client.GetAsync("/jokes/random").Result;
                    var stringData = response.Content.ReadAsStringAsync().Result;
                    var joke = JsonConvert.DeserializeObject<NorrisJoke>(stringData);

                    facts.Add(joke.value);
                }

            }

            return facts;
        }
    }
}
