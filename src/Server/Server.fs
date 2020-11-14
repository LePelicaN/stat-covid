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
      eprintfn "csvData: %A" csvData
      eprintfn "csvData.Headers: %A" csvData.Headers

      let firstRow = csvData.Rows |> Seq.head
      eprintfn "1st row: %A" firstRow
      eprintfn "1st row dep: %A" (firstRow.GetColumn "dep")
      eprintfn "1st row jour: %A" (firstRow.GetColumn "jour")


      let addNew (updatedPrevious:CovidStat list) (currentDay:CovidStat) =
        let updated =
          match updatedPrevious with
            | h::t ->
              { currentDay with
                  NewHospitalisation = currentDay.NbHospitalisation - h.NbHospitalisation
                  NewReanimation = currentDay.NbReanimation - h.NbReanimation
                  NewReturn = currentDay.NbReturn - h.NbReturn
                  NewDeath = currentDay.NewDeath - h.NewDeath
              }
            | _ ->
              { currentDay with
                  NewHospitalisation = currentDay.NbHospitalisation
                  NewReanimation = currentDay.NbReanimation
                  NewReturn = currentDay.NbReturn
                  NewDeath = currentDay.NewDeath
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
          NbHospitalisation = int (row.GetColumn CsvModel.nbHostitalisationLabel)
          NewHospitalisation = 0
          NbReanimation = int (row.GetColumn CsvModel.nbReanimationLabel)
          NewReanimation = 0
          NbReturn = int (row.GetColumn CsvModel.nbReturnLabel)
          NewReturn = 0
          NbDeath = int (row.GetColumn CsvModel.nbDeathLabel)
          NewDeath = 0
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
