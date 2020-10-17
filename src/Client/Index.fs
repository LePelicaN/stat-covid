module Index

open Elmish
open Fable.Remoting.Client
open Shared

type Model =
    { CovidStats: CovidStat list
      Input: string }

type Msg =
    | GotCovidStat of CovidStat list
    | SetInput of string
    | AddTodo

let covidStatApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ICovidStatApi>

let init(): Model * Cmd<Msg> =
    let model =
        { CovidStats = []
          Input = "" }
    let cmd = Cmd.OfAsync.perform covidStatApi.GetData ("69", "0") GotCovidStat
    model, cmd

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | GotCovidStat stats ->
        { model with CovidStats = stats }, Cmd.none
    | SetInput value ->
        { model with Input = value }, Cmd.none
    | AddTodo ->
        //let todo = Todo.create model.Input
        //let cmd = Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo
        // { model with Input = "" }, cmd
        { model with Input = "" }, Cmd.none
    // | AddedTodo todo ->
    //     { model with Todos = model.Todos @ [ todo ] }, Cmd.none

open Fable.React
open Fable.React.Props
open Fulma

let navBrand =
    Navbar.Brand.div [ ] [
        Navbar.Item.a [
            Navbar.Item.Props [ Href "https://safe-stack.github.io/" ]
            Navbar.Item.IsActive true
        ] [
            img [
                Src "/favicon.png"
                Alt "Logo"
            ]
        ]
    ]

let containerBox (model : Model) (dispatch : Msg -> unit) =
    Box.box' [ ] [
        Content.content [ ] [
            Content.Ol.ol [ ] [
                // for todo in model.Todos do
                //     li [ ] [ str todo.Description ]
            ]
        ]
        Field.div [ Field.IsGrouped ] [
            Control.p [ Control.IsExpanded ] [
                Input.text [
                  Input.Value model.Input
                  Input.Placeholder "What needs to be done?"
                  Input.OnChange (fun x -> SetInput x.Value |> dispatch) ]
            ]
            Control.p [ ] [
                Button.a [
                    Button.Color IsPrimary
                    // Button.Disabled (Todo.isValid model.Input |> not)
                    Button.OnClick (fun _ -> dispatch AddTodo)
                ] [
                    str "Add"
                ]
            ]
        ]
    ]

let dataPart (model : Model) =
  Table.table [ Table.IsHoverable ]
    [ thead [ ]
        [ tr [ ]
            [ th [ ] [ str "Date" ]
              th [ ] [ str "Hospitalisation" ]
              th [ ] [ str "Reanimation" ]
              th [ ] [ str "Return" ]
              th [ ] [ str "Death" ] ] ]
      tbody [ ] [
          for stat in model.CovidStats do
            tr [ ]
              [ td [ ] [ str (stat.Day.ToString("dd/MM/yyyy")) ]
                td [ ] [ str (string stat.NbHospitalisation) ]
                td [ ] [ str (string stat.NbReanimation) ]
                td [ ] [ str (string stat.NbReturn) ]
                td [ ] [ str (string stat.NbDeath) ] ]
      ]
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    Hero.hero [
        Hero.Color IsPrimary
        Hero.IsFullHeight
        Hero.Props [
            Style [
                Background """linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url("https://unsplash.it/1200/900?random") no-repeat center center fixed"""
                BackgroundSize "cover"
            ]
        ]
    ] [
        Hero.head [ ] [
            Navbar.navbar [ ] [
                Container.container [ ] [ navBrand ]
            ]
        ]

        Hero.body [ ] [
            Container.container [ ] [
                Column.column [
                    Column.Width (Screen.All, Column.Is6)
                    Column.Offset (Screen.All, Column.Is3)
                ] [
                    Heading.p [ Heading.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ] [ str "Statistic COVID" ]
                    containerBox model dispatch
                    dataPart model
                ]
            ]
        ]
    ]
