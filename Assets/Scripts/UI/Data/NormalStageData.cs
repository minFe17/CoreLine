using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using UnityEngine;
using JetBrains.Annotations;


[JsonConverter(typeof(StringEnumConverter))]
public enum ClearType
{
   MoneySave, HealthSave, UnitSave
}
[System.Serializable]
public struct WorldStageData
{
    public string Id;                       // 월드/챕터 ID (예: "Stage1")
    public string Name;                     // 월드 표시 이름 (예: "월드 1")
    public List<NormalStageData> Stages;    // 이 월드에 속한 스테이지들
}
[System.Serializable]
public struct NormalStageData
{
    public string Id; //스테이지 프리팹 로드용
    public string Name; //스테이지 선택 패널에서 이름 띄우는 용
    public List<Condition> Condition; //클리어타입 이넘으로 스위치 분기해서 판단
    public int Gold; //끝나고 얻는 재화
    public int Gem; //끝나고 얻는 재화
    public string UnlockCharacter; //클리어시 해금되는 캐릭터
}
[System.Serializable]
public struct Condition
{
    public ClearType ClearType; 
    public string Info; //스테이지 선택 패널에서 조건 보여주는 용      
    public float Value; // 엔딩씬에서 조건 체크할때 쓰는 밸류값          
}