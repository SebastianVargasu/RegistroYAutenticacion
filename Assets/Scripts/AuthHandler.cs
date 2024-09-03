using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Linq;
using UnityEngine.SocialPlatforms.Impl;

public class AuthHandler : MonoBehaviour
{
    string url = "https://sid-restapi.onrender.com";
    
    public string Token { get; set; }
    public string Username { get; set; }
    
    public Usuario[] Usuarios { get; set; }

    public GameObject UsernameField;
    public GameObject PasswordField;

    public GameObject PanelAuth;
    public GameObject PanelGame;
    public GameObject PanelUpdate;
    public GameObject PanelBoard;

    public GameObject ScoreField;
    public TextMeshProUGUI[] LeaderBordText;

    private bool _isUpdateActivate = false;
    private bool _isBoardActivate = false;

    public GameObject leaderBoardItemPrefab;
    private List<GameObject> leaderBoardItemList = new List<GameObject>();
    public GameObject leaderBoardParent;
    
    
    // Start is called before the first frame update
    void Start()
    {
        Token = PlayerPrefs.GetString("token");
        if (string.IsNullOrEmpty(Token))
        {
            PanelAuth.SetActive(true);
        }
        else
        {
            Username = PlayerPrefs.GetString("username");
            PanelAuth.SetActive(false);
            PanelGame.SetActive(true);
            //StartCoroutine("GetProfile");
        }
    }

    public void ActivateUpdatePanel()
    {
        if (!_isUpdateActivate)
        {
            PanelGame.SetActive(false);
            PanelUpdate.SetActive(true);
            _isUpdateActivate = true;
        }
        else
        {
            PanelGame.SetActive(true);
            PanelUpdate.SetActive(false);
            _isUpdateActivate = false;
        }
    }

    public void ActivateBoardPanel()
    {
        if (!_isBoardActivate)
        {
            PanelGame.SetActive(false);
            PanelBoard.SetActive(true);
            _isBoardActivate = true;
            StartCoroutine("GetUsers");
        }
        else
        {
            PanelGame.SetActive(true);
            PanelBoard.SetActive(false);
            _isBoardActivate = false;
        }
    }

    public void EnviarUpdatescore()
    {
        Usuario Usuario = new Usuario();
        Usuario.username = Username;
        UserDataApi data = new UserDataApi();
        
        if(int.TryParse(ScoreField.GetComponent<TMP_InputField>().text, out int score))
        {
            data.score = score;
        }
        else
        {
            data.score = 0;
        }

        Usuario.data = data;

        string jsonData = JsonUtility.ToJson(Usuario);

        StartCoroutine("UpdateScore", jsonData);
    }
    
    

    public void EnviarRegistro()
    {
        AuthData data = new AuthData();
        
        data.username = UsernameField.GetComponent<TMP_InputField>().text;
        data.password = PasswordField.GetComponent<TMP_InputField>().text;

        StartCoroutine("Registro", JsonUtility.ToJson(data));


    }

    public void EnviarLogin()
    {
        AuthData data = new AuthData();
        
        data.username = UsernameField.GetComponent<TMP_InputField>().text;
        data.password = PasswordField.GetComponent<TMP_InputField>().text;
        
        StartCoroutine("Login",JsonUtility.ToJson(data));
    }

    IEnumerator Registro(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(url + "/api/usuarios",json);
        request.method = UnityWebRequest.kHttpVerbPOST;
        
        request.SetRequestHeader("Content-Type","application/json");
        

        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            request.GetResponseHeader("Content-Type");
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                Debug.Log("Registro exitoso");
                StartCoroutine("Login", json);

            }
            else
            {
                Debug.Log(request.responseCode + " " + request.error );
            }
        }
    }

    IEnumerator Login(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(url + "/api/auth/login",json);
        request.method = UnityWebRequest.kHttpVerbPOST;
        
        request.SetRequestHeader("Content-Type","application/json");
        

        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            request.GetResponseHeader("Content-Type");
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                Debug.Log("Login Exitoso");
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
                
                Username = data.usuario.username;
                Token = data.token;
                
                PlayerPrefs.SetString("token", Token);
                PlayerPrefs.SetString("username", Username);
                
                PanelAuth.SetActive(false);
                PanelGame.SetActive(true);
                    
                //Debug.Log(data.token);

            }
            else
            {
               
                Debug.Log(request.responseCode + " " + request.error );
            }
        }
    }

    IEnumerator UpdateScore(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(url + "/api/usuarios",json);
        request.method = "PATCH";
        request.SetRequestHeader("x-token",Token);
        request.SetRequestHeader("Content-Type", "application/json");
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            
            if (request.responseCode == 200)
            {
                Debug.Log("Puntaje actualizado correctamente");
                ActivateUpdatePanel();
            }
            else
            {
               
                Debug.Log(request.responseCode + " " + request.error );
            }
        }
    }

    IEnumerator GetUsers()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/api/usuarios");
        request.SetRequestHeader("x-token",Token);
        foreach(GameObject k in leaderBoardItemList){
            Destroy(k);
        }
        leaderBoardItemList.Clear();
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                Debug.Log("Lista de usuarios obtenida");
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
                data.usuarios = data.usuarios.OrderByDescending(i => i.data.score).Take(5).ToArray();
                
                Usuarios= data.usuarios;

                for (int i = 0; i < Usuarios.Length; i++){
                    var g = Instantiate(leaderBoardItemPrefab, leaderBoardParent.transform);
                    g.GetComponent<LeaderBoardItem>().SetValues((i + 1).ToString(), Usuarios[i].username, Usuarios[i].data.score.ToString());
                    leaderBoardItemList.Add(g);
                }

            }
            else
            {
                Debug.Log(request.responseCode + " " + request.error );
            }
        }
    }
    
}

[System.Serializable]
public class AuthData
{
    public string username;
    public string password;
    public Usuario usuario;
    public Usuario[] usuarios;
    public string token;
}
[System.Serializable]
public class Usuario
{
    public string _id;
    public string username;
    public UserDataApi data;
}

[System.Serializable]
public class UserDataApi
{
    public int score;
}

