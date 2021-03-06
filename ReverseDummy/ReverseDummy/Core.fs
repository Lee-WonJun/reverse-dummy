namespace ReverseDummy

open System.Xml.Serialization
open System.Reflection
open System
open System.Linq

module internal InternalModule = 

    type Value = obj
    
    type Sentence =
        | Leaf of Leaf
        | Remain of Remain
    and Leaf = {name : string option ; objType : System.Type ; value : Value}
    and Remain = {name : string option ; objType : System.Type ; contain : Sentence[]}
    
    type ParseTag = Primitive | Reference | Collection
    
    let objTypeToParseTag ob = 
        let t = ob.GetType()
        match box ob with
        | :? int | :? double | :? float | :? string | :? bool | :? System.DateTime | :? Enum -> Primitive
        | :?  System.Collections.IEnumerable  -> Collection
        | _ -> Reference
    
    let getProps ob =
        let propWithoutXmlIgnore = 
            let pp = ob.GetType().GetProperties() // 확인용
            ob.GetType().GetProperties()
            |> Array.filter (fun p -> Array.exists (fun (a:Attribute) -> (a :? XmlIgnoreAttribute)) (p.GetCustomAttributes().ToArray()) |> not) 
        propWithoutXmlIgnore
    
    
    let rec PropertyToSentence ob (property : PropertyInfo) =
        let name = property.Name
        let objType = property.PropertyType
        let value = property.GetValue(ob)
        
        let ParseType = objTypeToParseTag value
    
        match ParseType with
        | Primitive -> Leaf {name = Some name ; objType = objType ; value = value}
        | Reference -> Remain {name = Some name ; objType = objType ; contain = ClassToSentence(value)}
        | Collection ->  Remain {name = Some name ; objType = objType ; contain = ClassToSentence(value)}
    and ClassToSentence ob =
        let ParseType = objTypeToParseTag ob
        let props = getProps ob
    
        match ParseType with
        | Primitive -> [|Leaf {name = None ; objType = ob.GetType() ; value = ob}|]
        | Reference -> props|> Array.map (PropertyToSentence ob)
        | Collection -> 
            let col = Seq.cast (ob :?> System.Collections.IEnumerable) |> Array.ofSeq
            if not (Seq.isEmpty col) then
                let childType =  col.ElementAt(0).GetType();
                col |> Array.map ClassToSentence 
                |> Array.map (fun sen -> Remain {name = None ; objType = childType; contain = sen} )
            else
                [|Leaf {name = None ; objType = ob.GetType() ; value = ob}|]
    
        
    
    let rec SentenceToCSharpCode = 
        function
        | Leaf(x) -> 
            let {name= name;objType=objType;value=value} = x
            let preName = name |> function Some x -> sprintf "%s = " x | None -> ""
            
            match value with
            | :? System.DateTime as t ->  sprintf "%s new DateTime(%d)" preName t.Ticks
            | :? Enum as en -> sprintf "%s %A.%A" preName objType value
            | _ -> sprintf "%s %A" preName value
            
        | Remain(x) ->
            let {name= name;objType=objType;contain=contain} = x
            let preName = name |> function Some x -> sprintf "%s = " x | None -> ""
    
            let containString = 
                Array.map SentenceToCSharpCode contain 
                |> (fun array -> if Array.isEmpty array then "" else  Array.reduce (fun s1 s2 -> sprintf "%s ,\n %s" s1 s2) array)
            
            sprintf "%s  new %s \n{ \n%s \n}" preName objType.FullName containString
    

module Generator = 
    open InternalModule
    let ToCSharpCode nameOfVar ob =
        let sentence = ClassToSentence ob
        let first = Remain {name = Some nameOfVar ; objType = ob.GetType(); contain = sentence} 
        let code = SentenceToCSharpCode first
        sprintf "var %s ;" code