using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UIElements;
using UnityEngine.Networking;

public class ParseIIIF : MonoBehaviour
{
    public string path; 
    WWW file;

    List<string> content;
    public Sprite imageTexture;

    //tester
    //https://digi.vatlib.it/iiif/MSS_Vat.lat.1125/manifest.json

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadRemoteURL(path));
    }

    IEnumerator LoadRemoteURL(string path)
    {
        file = new WWW(path);
        yield return file;

        if (file.error == null)
        {
            ParseJson( ( file.text));
        }
        else
        {
            Debug.Log("ERROR: invalid path" + path);
        }
    }

    /* Read and store json values */
    void ParseJson(string data) {

        Manifest json = JsonConvert.DeserializeObject<Manifest>(data);
        print(json.sequences[0].canvases.Count); // use dictionaries

        List<string> uri_list = new List<string>();


        //json.sequences[0].canvases[0].images[0].resource.service["@id"];
        //get the image uri
        //eg. https://digi.vatlib.it/iiifimage/MSS_Vat.lat.1125/Vat.lat.1125_0004_cy_0001v.jp2/0,0,2128,3072/266,/0/native.jpg

        //foreach (var canvas in json.sequences[0].canvases) {
            string temp = json.sequences[0].canvases[2].images[0].resource.service["@id"] + "/0,0,2128,3072/266,/0/native.jpg";
            
            DownloadImage(temp);
        //}

        print("length " + uri_list.Count);



    }

    public void DownloadImage(string url) {
         
        StartCoroutine(ImageRequest(url, (UnityWebRequest req) => {
            if (req.isHttpError || req.isNetworkError)
            {
                Debug.Log($"{req.error}: {req.downloadHandler.text}");
            }
            else {
                Texture2D texture = DownloadHandlerTexture.GetContent(req);
                imageTexture = Sprite.Create(texture, new Rect(0,0, texture.width, texture.height), new Vector2(0.5f,0.5f));
            }
        }));
    }


    IEnumerator ImageRequest(string path, System.Action<UnityWebRequest> callback) {
        using (UnityWebRequest req =  UnityWebRequestTexture.GetTexture(path)) {
            yield return req.SendWebRequest();
            callback(req);
        }
    
    }
}

/* JSON Structs */

[System.Serializable]
public class Manifest
{
    public string context;
    public string id; 
    public string type;
    public string label;
    public List<Metadata> metadata;
    public List<Description> description;
    public string license;
    public string attribution;
    public string logo;
    public List<Service> service;
    public string related;
    public string within;
    public List<Sequence> sequences;
    public List<string> structure; 
}

[System.Serializable]
public class Service {
    public string context;
    public string id;
    public string profile;
    public string label;
}

[System.Serializable]
public class Description {
    public string value;
    public string language; 
}

[System.Serializable]
public class Metadata {
    public string label;
}

[System.Serializable]
public class Sequence
{
    public string @id;
    public string @type;
    public List<string> label; //probably not going to use
    public List<Canvas> canvases;
}

[System.Serializable]
public class Canvas
{
    public string @id;
    public string @type;
    public string label;
    public int width;
    public int height; 
    public List<Image> images;
}

[System.Serializable]
public class Image {
    public string id;
    public string type; 
    public string on;
    public string motivation;
    public Resources resource { get; set; }
}

[System.Serializable]
public class Resources {
    public string @id;
    public string @type;
    public string width;
    public int height;
    public string format;
    public Dictionary<string, string> service { get; set; }
}