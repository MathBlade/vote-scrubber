using System;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Collections.Generic;
using static VoteAPIAllData;

public static class RestHelper
{


    public static MasterData GetMasterData(string path)
    {
        HttpResponseMessage response = Task.Run(() => ForumMafiaRestOnly.Service1.HTTPClient.GetAsync(path)).Result;
       if (response.IsSuccessStatusCode)
        {
            return Task.Run(() => response.Content.ReadAsAsync<MasterData>()).Result;
        }

        return null;
        /*MasterData md = null;
        HttpResponseMessage response = await ForumMafiaRestOnly.Service1.HTTPClient.GetAsync(path);
        if (response.IsSuccessStatusCode)
        {
            md = await response.Content.ReadAsAsync<MasterData>();
        }
        return md;*/
    }



    
    
}
