module Index

open Elmish
open Fable.Import.Chartjs
open Fable.Remoting.Client
open Shared
open Shared.CovidStatHelper

type DisplayChoice =
  | Nb
  | New
  | Acceleration

let NbLabel = "Number"
let NewLabel = "New"
let AccelerationLabel = "Acceleration"

let parseDisplayChoice = function
 | x when x = NbLabel -> Nb
 | x when x = NewLabel -> New
 | x when x = AccelerationLabel -> Acceleration
 | _ -> raise (System.ArgumentException("Invalid display choice!"))

type Model =
    { CovidStats: CovidStat list
      County: string
      Sex: string
      NewWindow: int
      AccelerationWindow: int
      TableDisplayChoice: DisplayChoice }

type Msg =
    | GotCovidStat of CovidStat list
    | SetCounty of string
    | SetSex of string
    | SetNewWindow of string
    | SetAccelerationWindow of string
    | SetTableDisplayChoice of string
    | Refresh

let covidStatApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ICovidStatApi>

let synchroCmd model =
  Cmd.OfAsync.perform covidStatApi.GetData (model.County, model.Sex, model.NewWindow, model.AccelerationWindow) GotCovidStat

let init(): Model * Cmd<Msg> =
    let model =
        { CovidStats = []
          County = "69"
          Sex = "0"
          NewWindow = 7
          AccelerationWindow = 3
          TableDisplayChoice = Nb }
    model, synchroCmd model

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | GotCovidStat stats ->
      { model with CovidStats = stats }, Cmd.none
    | SetCounty value ->
      { model with County = value }, Cmd.none
    | SetSex value ->
      { model with Sex = value }, Cmd.none
    | SetNewWindow value ->
      { model with NewWindow = int(value) }, Cmd.none
    | SetAccelerationWindow value ->
      { model with AccelerationWindow = int(value) }, Cmd.none
    | SetTableDisplayChoice value ->
      { model with TableDisplayChoice = parseDisplayChoice(value) }, Cmd.none
    | Refresh ->
      model, synchroCmd model

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
        Field.div [ ] [
            Label.label [ ]
                  [ str "County : " ]
            Control.div [ ] [
                Input.text [
                  Input.Value model.County
                  Input.Placeholder "County to analyse"
                  Input.OnChange (fun x -> SetCounty x.Value |> dispatch) ]
            ]
            Label.label [ ]
                  [ str "Sex : " ]
            Select.select [ ]
                [ select
                    [ DefaultValue "0"
                      OnChange (fun x -> SetSex x.Value |> dispatch) ]
                    [ option [ Value "0" ] [ str "Both" ]
                      option [ Value "1"] [ str "Men" ]
                      option [ Value "2"] [ str "Women" ] ]
                 ]
            Label.label [ ]
                  [ str "New window : " ]
            Control.div [ ] [
                Input.number [
                  Input.Value <| model.NewWindow.ToString()
                  Input.Placeholder "Window to compute new"
                  Input.OnChange (fun x -> SetNewWindow x.Value |> dispatch) ]
            ]
            Label.label [ ]
                  [ str "Acceleration window : " ]
            Control.div [ ] [
                Input.number [
                  Input.Value <| model.AccelerationWindow.ToString()
                  Input.Placeholder "Window to compute acceleration"
                  Input.OnChange (fun x -> SetAccelerationWindow x.Value |> dispatch) ]
            ]
            Control.p [ ] [
                Button.a [
                    Button.Color IsPrimary
                    // Button.Disabled (Todo.isValid model.Input |> not)
                    Button.OnClick (fun _ -> dispatch Refresh)
                ] [
                    str "Refresh"
                ]
            ]
        ]
    ]

let datarepresentationchoice (model : Model) (dispatch : Msg -> unit) =
  Box.box' [ ] [
    Field.div [ ] [
      Select.select [ ]
            [ select
                [ DefaultValue "Number"
                  OnChange (fun x -> SetTableDisplayChoice x.Value |> dispatch) ]
                [ option [ Value NbLabel ] [ str "Number" ]
                  option [ Value NewLabel ] [ str "New" ]
                  option [ Value AccelerationLabel ] [ str "Acceleration" ] ]
             ]
    ]
  ]

let renderChart title (model : Model) =
  testChartProps

let dataPart (model : Model) =
  Container.container [
    Container.IsFluid
    Container.Modifiers [ Modifier.BackgroundColor IsGreyLighter ]
  ]
    [ 
      renderChart "test" model

      Table.table [ Table.IsHoverable ]
        [ thead [ ]
            [ tr [ ]
                [ th [ ] [ str "Date" ]
                  th [ ] [ str "Hospitalisation" ]
                  th [ ] [ str "Reanimation" ]
                  th [ ] [ str "Return" ]
                  th [ ] [ str "Death" ] ] ]
          tbody [ ] [
              let getterData, getterOption =
                match model.TableDisplayChoice with
                  | Nb -> getNbOnDay, None
                  | New -> getNewOnDay, Some getNewOnDay2
                  | Acceleration -> getAcceleration, Some getAcceleration2
              for stat in model.CovidStats do
                tr [ ]
                  [
                    td [ ] [ str (stat.Day.ToString("dd/MM/yyyy")) ]
                    if Option.isNone <| getterOption ||
                      Option.isSome <| getterOption.Value stat then
                      td [ ] [ str (sprintf "%.2f" (stat |> getterData |> getHospitalisation) ) ]
                      td [ ] [ str (sprintf "%.2f" (stat |> getterData |> getReanimation) ) ]
                      td [ ] [ str (sprintf "%.2f" (stat |> getterData |> getReturn) ) ]
                      td [ ] [ str (sprintf "%.2f" (stat |> getterData |> getDeath) ) ]
                  ]
          ]
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
                    //button [ Id "test"] [ str "asf" ]
                    canvas [ Id "chart-id" ] []
                    //  <canvas id="chart"></canvas>
                    datarepresentationchoice model dispatch
                    dataPart model
                ]
            ]
        ]
    ]
    
