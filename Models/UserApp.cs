using System;
using Newtonsoft.Json;

namespace OidcSampleApp.Models
{
    public class UserApp
    {
      [JsonProperty("id")]
      public int Id { get; set; }

      [JsonProperty("name")]
      public string Name { get; set; }

      [JsonProperty("icon_url")]
      public string Icon { get; set; }

      [JsonProperty("provisioning_enabled")]
      public bool ProvisionEnabled { get; set; }

      [JsonProperty("extension")]
      public bool Extension { get; set; }

      [JsonProperty("login_id")]
      public string LoginId { get; set; }

      [JsonProperty("provisioning_status")]
      public string ProvisionStatus { get; set; }

      [JsonProperty("provisioning_state")]
      public string ProvisionState { get; set; }
    }
}