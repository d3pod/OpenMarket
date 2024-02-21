using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace openmarket.Models
{
    public class Facebook
    {
        readonly string _accessToken;
        readonly string _pageID;
        readonly string _facebookAPI = "https://graph.facebook.com/";
        readonly string _pageEdgeFeed = "feed";
        readonly string _pageEdgePhotos = "photos";
        readonly string _postToPageURL;
        readonly string _postToPagePhotosURL;
        public Facebook(string accessToken, string pageID)
        {
            _accessToken = accessToken;
            _pageID = pageID;
            _postToPageURL = $"{_facebookAPI}{pageID}/{_pageEdgeFeed}";
            _postToPagePhotosURL = $"{_facebookAPI}{pageID}/{_pageEdgePhotos}";
        }

        public async Task<Tuple<int, string>> PublishSimplePost(string postText, string link, string published, string scheduled_publish_time)
        {
            using (var http = new HttpClient())
            {
                var postData = new Dictionary<string, string> {
                { "access_token", _accessToken },
                { "message", postText },
                { "link", link }/* ,
                { "published", "false"},
                { "scheduled_publish_time", scheduled_publish_time} */
            };

                var httpResponse = await http.PostAsync(
                    _postToPageURL,
                    new FormUrlEncodedContent(postData)
                    );
                var httpContent = await httpResponse.Content.ReadAsStringAsync();

                return new Tuple<int, string>(
                    (int)httpResponse.StatusCode,
                    httpContent
                    );
            }
        }

        public async Task<Tuple<int, string>> UploadPhoto(string photoURL, string published, string scheduled_publish_time)
        {
            using (var http = new HttpClient())
            {
                var postData = new Dictionary<string, string> {
                { "access_token", _accessToken },
                { "url", photoURL },
                { "is_published", "false"}
            };

                var httpResponse = await http.PostAsync(
                    _postToPagePhotosURL,
                    new FormUrlEncodedContent(postData)
                    );
                var httpContent = await httpResponse.Content.ReadAsStringAsync();

                return new Tuple<int, string>(
                    (int)httpResponse.StatusCode,
                    httpContent
                    );
            }
        }
        public async Task<Tuple<int, string>> UpdatePhotoWithPost(string postID, string postText, string link, string published, string scheduled_publish_time)
        {
            using (var http = new HttpClient())
            {
                var postData = new Dictionary<string, string> {
                { "access_token", _accessToken },
                { "message", postText },
                { "link", link }
            };

                var httpResponse = await http.PostAsync(
                    $"{_facebookAPI}{postID}",
                    new FormUrlEncodedContent(postData)
                    );
                var httpContent = await httpResponse.Content.ReadAsStringAsync();

                return new Tuple<int, string>(
                    (int)httpResponse.StatusCode,
                    httpContent
                    );
            }
        }

        public string PublishToFacebook(string postText, string link, string published, string scheduled_publish_time)
        {
            try
            {
                var rezText = Task.Run(async () =>
                {
                    using (var http = new HttpClient())
                    {
                        return await PublishSimplePost(postText, link, published, scheduled_publish_time);
                    }
                });
                var rezTextJson = JObject.Parse(rezText.Result.Item2);

                if (rezText.Result.Item1 != 200)
                {
                    try // return error from JSON
                    {
                        return $"Error posting to Facebook. {rezTextJson["error"]["message"].Value<string>()}";
                    }
                    catch (Exception ex) // return unknown error
                    {
                        // log exception somewhere
                        return $"Unknown error posting to Facebook. {ex.Message}";
                    }
                }

                return "OK";
            }
            catch (Exception ex)
            {
                // log exception somewhere
                return $"Unknown error publishing post to Facebook. {ex.Message}";
            }
        }
    }
}