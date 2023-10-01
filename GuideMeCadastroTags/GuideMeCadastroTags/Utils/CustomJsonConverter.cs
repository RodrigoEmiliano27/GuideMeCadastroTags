using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace GuideMeCadastroTags.Utils
{
    public class CustomJsonConverter : JsonConverter<ResultCallApi>
    {
        public override ResultCallApi ReadJson(JsonReader reader, Type objectType, ResultCallApi existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // Customize the deserialization logic here
            // You can parse the JSON manually and create a ResultCallApi object

            // Example:
            var jsonObject = JObject.Load(reader);

            var result = new ResultCallApi
            {
                Retorno = (string)jsonObject["Retorno"],
                RetornoObj = jsonObject["RetornoObj"],
                Erro = null, // You can customize the deserialization of the Exception here
                Sucesso = (bool)jsonObject["Sucesso"],
                MsgErro = (string)jsonObject["MsgErro"]
            };

            return result;
        }

        public override void WriteJson(JsonWriter writer, ResultCallApi value, JsonSerializer serializer)
        {
            // Customize the serialization logic here

                    // Example:
                    var jsonObject = new JObject
                {
                    { "Retorno", value.Retorno },
                    { "RetornoObj", JToken.FromObject(value.RetornoObj, serializer) }, // Serialize RetornoObj
                    { "Erro", JToken.FromObject(value.Erro, serializer) }, // Serialize Erro
                    { "Sucesso", value.Sucesso },
                    { "MsgErro", value.MsgErro }
                };

            jsonObject.WriteTo(writer);
        }
    }
}
