module Server

open System

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared

open FSharp.Data

open CsvModel

// type Storage () =
    //let todos = ResizeArray<_>()

    // member __.GetTodos () =
    //     List.ofSeq todos

    // member __.AddTodo (todo: Todo) =
    //     if Todo.isValid todo.Description then
    //         todos.Add todo
    //         Ok ()
    //     else Error "Invalid todo"

// let storage = Storage()

// storage.AddTodo(Todo.create "Create new SAFE project") |> ignore
// storage.AddTodo(Todo.create "Write your app") |> ignore
// storage.AddTodo(Todo.create "Ship it !!!") |> ignore

let todosApi = {
  GetData = fun (county:string, sexe:string) -> async {
    try
      let address = "https://www.data.gouv.fr/fr/datasets/r/63352e38-d353-4b54-bfd1-f1b3ee1cabd7"
      let csvData = CsvFile.Load(address, ";")

      let addNew (updatedPrevious:CovidStat list) (currentDay:CovidStat) =
        let updated =
          match updatedPrevious with
            | h::t ->
              { currentDay with
                  NewOnDay = Some
                    {
                      Hospitalisation = currentDay.NbOnDay.Hospitalisation - h.NbOnDay.Hospitalisation
                      Reanimation = currentDay.NbOnDay.Reanimation - h.NbOnDay.Reanimation
                      Return = currentDay.NbOnDay.Return - h.NbOnDay.Return
                      Death = currentDay.NbOnDay.Death - h.NbOnDay.Death
                    }
              }
            | _ ->
              { currentDay with
                  NewOnDay = Some
                    {
                      Hospitalisation = currentDay.NbOnDay.Hospitalisation
                      Reanimation = currentDay.NbOnDay.Reanimation
                      Return = currentDay.NbOnDay.Return
                      Death = currentDay.NbOnDay.Death
                    }
              }
        updated :: updatedPrevious

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
      |> Seq.fold addNew []
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
