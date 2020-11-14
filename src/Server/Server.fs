module Server

open System

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared
open Shared.CovidStatHelper

open FSharp.Data

open CsvModel

let todosApi = {
  GetData = fun (county:string, sexe:string, newWindow:int, accelerationWindow:int) -> async {
    try
      let address = "https://www.data.gouv.fr/fr/datasets/r/63352e38-d353-4b54-bfd1-f1b3ee1cabd7"
      let csvData = CsvFile.Load(address, ";")

      let updateStats (updatedPrevious:CovidStat list) (currentDay:CovidStat) =
        let updatedWithNew =
          if newWindow > 0 && updatedPrevious.Length >= newWindow then
            let prevWindow = updatedPrevious.[newWindow - 1]
            // eprintf "prevWindow: %A" prevWindow
            // eprintf "currentDay: %A" currentDay
            { currentDay with
                NewOnDay = Some <| diffStats getNbOnDay currentDay prevWindow / (float newWindow)
            }
          else currentDay
        let updatedWithAcceleration =
          if accelerationWindow > 0 && updatedPrevious.Length >= accelerationWindow then
            let prevWindow = updatedPrevious.[accelerationWindow - 1]
            if Option.isSome <| getNewOnDay2 prevWindow then
              { updatedWithNew with
                  Acceleration = Some <| diffStats getNewOnDay updatedWithNew prevWindow / (float accelerationWindow)
              }
            else updatedWithNew
          else updatedWithNew
        updatedWithAcceleration :: updatedPrevious

      return csvData.Rows
      |> Seq.filter (fun row ->
        (row.GetColumn CsvModel.countyLabel) = county &&
          (row.GetColumn CsvModel.sexLabel) = sexe
      )
      |> Seq.map (fun row -> 
        {
          Day = DateTime.ParseExact((row.GetColumn CsvModel.dateLabel), CsvModel.dateFormats, null, System.Globalization.DateTimeStyles.None)
          NbOnDay =
            {
              Hospitalisation = float (row.GetColumn CsvModel.nbHostitalisationLabel)
              Reanimation = float (row.GetColumn CsvModel.nbReanimationLabel)
              Return = float (row.GetColumn CsvModel.nbReturnLabel)
              Death = float (row.GetColumn CsvModel.nbDeathLabel)
            }
          NewOnDay = Option.None
          Acceleration = Option.None
        }
      )
      |> Seq.fold updateStats []
      |> List.rev

    with
      | ex ->
        eprintfn "Exception: %A" ex
        return []
  }
}

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue todosApi
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
