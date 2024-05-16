//#define DBG_LOCAL

using MRK.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace MRK
{
    public class Client
    {
        private readonly HttpClient _httpClient;
        private Session? _currentSession;

        private static Client? _instance;

        /// <summary>
        /// Current session, make sure you are logged in before accessing
        /// </summary>
        public Session CurrentSession => _currentSession!;

        /// <summary>
        /// Public client instance
        /// </summary>
        public static Client Instance { get => _instance ??= new(); }

        public Client()
        {
            _httpClient = new HttpClient();
        }

        #region Auth Controller

        public async Task<bool> Login(string username, string pwd)
        {
            // do get req, etc
#if DBG_LOCAL
            _currentSession = new Session(
                "sessionidherelol",
                new User(
                    "indiuewuiwhe2s1sw",
                    "mrkdagods",
                    "mamar452@gmail.com",
                    "2321221OC"));

            await Task.Delay(500);
            return true;
#else
            var uri = BuildRequestURI("auth/login");
            var sessionResult = await _httpClient.PostAsJsonAsync(uri, new
            {
                username,
                password = pwd
            });

            if (sessionResult.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            dynamic session = JsonConvert.DeserializeObject(await sessionResult.Content.ReadAsStringAsync())!;

            // fetch user data
            uri = BuildRequestURI("auth/fetch");
            var fetchResult = await _httpClient.PostAsJsonAsync(uri, new
            {
                userId = (string)session.userId
            });

            if (fetchResult.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            var user = JsonConvert.DeserializeObject<User>(await fetchResult.Content.ReadAsStringAsync());
            if (user == null)
            {
                MessageBox.Show("Cannot retrieve user data");
                return false;
            }

            _currentSession = new Session((string)session.id, user);
            return true;
#endif
        }

        public async Task<bool> Register(string username, string email, string pwd)
        {
            // do get req, etc
#if DBG_LOCAL
            await Task.Delay(400);
            return true;
#else
            var uri = BuildRequestURI("auth/register");
            var result = await _httpClient.PostAsJsonAsync(uri, new
            {
                username,
                email,
                password = pwd
            });

            return result.StatusCode == HttpStatusCode.OK;
#endif
        }

        public async Task Logout()
        {
            if (_currentSession != null)
            {
#if !DBG_LOCAL
                var uri = BuildRequestURI("auth/logout");
                await _httpClient.PostAsJsonAsync(uri, new
                {
                    sessionId = _currentSession.Id
                });
#else
                await Task.Delay(500);
#endif

                _currentSession = null;
            }
        }

        public async Task<List<User>> SearchUsers(string query)
        {
            if (_currentSession == null) return [];

            var uri = BuildRequestURI("auth/search");
            var result = await _httpClient.PostAsJsonAsync(uri, new
            {
                query
            });

            if (result.StatusCode != HttpStatusCode.OK) 
            {
                return [];
            }

            return JsonConvert.DeserializeObject<List<User>>(await result.Content.ReadAsStringAsync())!;
        }

        #endregion

        #region Document Controller
        public async Task<bool> CreateDocument(string name)
        {
            if (_currentSession == null) return false;

            var uri = BuildRequestURI("doc/create");
            var result = await _httpClient.PostAsJsonAsync(uri, new
            {
                sessionId = _currentSession.Id,
                name
            });

            return result.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> RenameDocument(Document doc, string name)
        {
            if (_currentSession == null) return false;

            var uri = BuildRequestURI("doc/rename");
            var result = await _httpClient.PostAsJsonAsync(uri, new
            {
                sessionId = _currentSession.Id,
                docId = doc.Id,
                name
            });

            return result.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> DeleteDocument(Document doc)
        {
            if (_currentSession == null) return false;

            var uri = BuildRequestURI("doc/delete");
            var result = await _httpClient.PostAsJsonAsync(uri, new
            {
                sessionId = _currentSession.Id,
                docId = doc.Id
            });

            return result.StatusCode == HttpStatusCode.OK;
        }

        public async Task<List<Document>> GetDocuments()
        {
            if (_currentSession == null) return [];

            var uri = BuildRequestURI("doc/docs");
            var result = await _httpClient.PostAsJsonAsync(uri, new
            {
                sessionId = _currentSession.Id
            });

            if (result.StatusCode != HttpStatusCode.OK)
            {
                return [];
            }

            var docs = JsonConvert.DeserializeObject<List<dynamic>>(await result.Content.ReadAsStringAsync())!;

            // get edit perms
            uri = BuildRequestURI("doc/checkperms");
            var permsResult = await _httpClient.PostAsJsonAsync(uri, new
            {
                sessionId = _currentSession.Id,
                docIds = docs.Select(x => (string)x.id).ToList()
            });

            if (permsResult.StatusCode != HttpStatusCode.OK)
            {
                return [];
            }

            var perms = await permsResult.Content.ReadFromJsonAsync<Dictionary<string, bool>>();

            return docs.Select(x => new Document(
                (string)x.id,
                (string)x.name,
                (string)x.ownerId,
                (DateTime)x.creationDate,
                (DateTime)x.modificationDate,
                perms![(string)x.id])
            ).ToList();
        }

        public async Task<Dictionary<User, bool>> GetDocumentUsers(Document document)
        {
            if (_currentSession == null) return [];

            var uri = BuildRequestURI("doc/users");
            var result = await _httpClient.PostAsJsonAsync(uri, new
            {
                sessionId = _currentSession.Id,
                docId = document.Id

            });

            if (result.StatusCode != HttpStatusCode.OK)
            {
                return [];
            }

            var res = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(await result.Content.ReadAsStringAsync())!;

            return res
                .Select(x => KeyValuePair.Create(((JObject)x.Value.user).ToObject<User>()!, (bool)x.Value.write))
                .ToDictionary();
        }

        public async Task<bool> AddUserToDocument(User user, Document doc, bool edit)
        {
            if (_currentSession == null) return false;

            var uri = BuildRequestURI("doc/adduser");
            var result = await _httpClient.PostAsJsonAsync(uri, new
            {
                sessionId = _currentSession.Id,
                docId = doc.Id,
                userId = user.Id,
                edit
            });

            return result.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> DeleteUserFromDocument(User user, Document doc)
        {
            if (_currentSession == null) return false;

            var uri = BuildRequestURI("doc/deleteuser");
            var result = await _httpClient.PostAsJsonAsync(uri, new
            {
                sessionId = _currentSession.Id,
                docId = doc.Id,
                userId = user.Id
            });

            return result.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> ModifyUserInDocument(User user, Document doc, bool edit)
        {
            if (_currentSession == null) return false;

            var uri = BuildRequestURI("doc/modifyuser");
            var result = await _httpClient.PostAsJsonAsync(uri, new
            {
                sessionId = _currentSession.Id,
                docId = doc.Id,
                userId = user.Id,
                edit
            });

            return result.StatusCode == HttpStatusCode.OK;
        }
        #endregion

        private static Uri BuildRequestURI(string request)
        {
            return new Uri($"http://{Constants.BackendIP}:{Constants.BackendPort}/{request}");
        }
    }
}
