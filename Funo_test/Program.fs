module Funogram.Examples.HelloWorld.Program

open Funogram.Api
open Funogram.Telegram
open Funogram.Telegram.Bot
open FSharp.Control.Reactive
open System
open System.Reactive.Linq
open System.Windows.Input 


type State2 = {Subscribers:(string -> unit) list}

type Msg2 =
    | SendAll of string
    | Subscribe of (string -> unit)

let chatAgent = MailboxProcessor<Msg2>.Start(fun inbox->
    // функция обработки сообщения
    let rec messageLoop state = async{
        // чтение сообщения
        let! message = inbox.Receive()
        match message with
        | SendAll msg-> 
            printfn $"{msg}"
            for x in state.Subscribers do x msg
            do! messageLoop state
        | Subscribe x -> 
            let newstate = { state with Subscribers = x::state.Subscribers }  
            printfn $"subscribe"
            do! messageLoop newstate
        return! messageLoop state
        }
    // запуск рекурсии
    messageLoop {Subscribers=[] } )

let siteUri = new System.Uri("https://wiki.merionet.ru/images/vse-chto-vam-nuzhno-znat-pro-devops/1.png")




let updateArrived (ctx: UpdateContext) =
  match ctx.Update.Message with
  | Some { MessageId = messageId; Chat = chat; From = from } -> 
    //let some1 = Types.InputFile.Url siteUri
    //Api.sendDocument chat.Id some1 "test2" |> api ctx.Config
    //|> Async.Ignore
    //|> Async.Start
    let someFunc x =
        Api.sendMessage chat.Id x
        |> api ctx.Config
        |> Async.Ignore
        |> Async.Start
    chatAgent.Post(Subscribe someFunc)
  | _ -> ()

let keyRead = Console.ReadKey()
let keyChar = keyRead.KeyChar
printfn"Key Char is : %c" keyChar
let stringChar = keyChar|>string
chatAgent.Post(SendAll stringChar)




[<EntryPoint>]
let main _ =
  async {
    let config = Config.defaultConfig |> Config.withReadTokenFromFile
    let! _ = Api.deleteWebhookBase () |> api config
    return! startBot config updateArrived None
  } |> Async.RunSynchronously
  0