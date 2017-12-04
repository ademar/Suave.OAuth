﻿module internal Suave.HttpCli

open System.IO
open System.Net

type DefineRequest = HttpWebRequest -> unit

let send (url : string) meth (define : DefineRequest) = 
    async { 
        let request = WebRequest.Create(url) :?> HttpWebRequest
        request.Method <- meth
        request.UserAgent <- "suave app"    // this line is required for github auth

        do define request

        let! (r:WebResponse) = request.GetResponseAsync() |> Async.AwaitTask
        use response = r
        let stream = response.GetResponseStream()
        use reader = new StreamReader(stream)
        return reader.ReadToEnd()
    }

let post (url : string) (define : DefineRequest) (data : byte []) = 
    send url "POST"
        (fun request ->
        request.ContentType <- "application/x-www-form-urlencoded"
        request.ContentLength <- int64 data.Length

        do define request
        
        use stream = request.GetRequestStream()
        stream.Write(data, 0, data.Length)
        stream.Close()

)

let get (url : string) = send url "GET"
