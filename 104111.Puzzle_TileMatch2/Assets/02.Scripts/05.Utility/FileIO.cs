#region ★ Introduction

/// <summary>Registry Class Introduction</summary>
///  
/// ★ Information
///  - Author : Pinelia Luna
///  - Update : 2021.08.23
///  - Version : 2.0.2
///  - Contact : pinel0102@gmail.com
///  
/// ★ Summary
///  - FileIO 클래스 사용을 보조하는 클래스입니다.
///  - 파일의 텍스트 내용을 담은 wholeString 을 new Registry() 에서 관리합니다.
///  - 지원 타입 : string, int, float, bool, enum
///  - 자세한 사용 방법은 FileIO 클래스를 참조하면 됩니다.
///  - 이 클래스는 유니티 에서만 사용할 수 있습니다.
///  
/// ★ How to use 
///  - Registry reg = new Registry(); 를 통해 새로운 Registry 를 생성합니다.
///  - reg.MethodName(); 과 같은 방식으로 사용합니다.
///  
/// ★ Text-file Form
///  　#Section1
///  　　Key1 = Value1
///  　　Key2 = Value2
///  　　
///  　#Section2
///  　　Key1 = Value1
///  　　Key2 = Value2
///  　　
/// ★ Methods
///  - Constructor
///   - public Registry()								// 새로운 Registry 를 만듭니다.
///  
///  - Handle File
///   - public void LoadFileInPersistentDataPath()		// [PersistentDataPath]/sourcePath 파일의 텍스트 내용을 Registry 에 저장합니다.
///   - public void LoadFileInStreamingAssets()			// [StreamingAssets]/sourcePath 파일의 텍스트 내용을 Registry 에 저장합니다.
///   - public void LoadFileInResources()				// [Resources]/sourcePath 파일의 텍스트 내용을 Registry 에 저장합니다.
///   - public void SaveFileInPersistentDataPath()		// Registry 의 내용을 [PersistentDataPath]/targetPath 파일에 저장합니다.
///   - public void SaveFileInStreamingAssets()			// Registry 의 내용을 [StreamingAssets]/targetPath 파일에 저장합니다.
///   - public void SaveFileInResources()				// Registry 의 내용을 [Resources]/targetPath 파일에 저장합니다.
///   - public void BackupFile()						// [PersistentDataPath]/[bak] 폴더에 [PersistentDataPath]/sourcePath 파일의 백업본을 저장합니다.
///  
///  - Handle Value
///   - public void GetValue()							// Registry 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 불러옵니다.
///   - public void SetValue()							// Registry 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 변경합니다.
///   - public [string, float, int, bool] GetValue()	// Registry 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 불러옵니다.
/// 
///  - Handle Form
///   - public void AddSection()						// Registry 의 하단에 #thisSection 을 추가합니다.
///   - public void DelSection()						// #thisSection 과 하단의 모든 key=value 를 삭제합니다.
///   - public void AddKeyValue()						// #thisSection 의 하단에 thisKey=newValue 형태의 라인을 추가합니다.
///   - public void DelKeyValue()						// #thisSection 의 하단에 thisKey=newValue 형태의 라인을 삭제합니다.
///   - public bool SectionIsExist()					// Registry 에 #thisSection 이 존재하는지 확인합니다.
///   - public bool KeyIsExist()						// #thisSection 의 하단에 thisKey 가 존재하는지 확인합니다.
///  

/// <summary>FileIO Class Introduction</summary>
/// 
/// ★ Information
///  - Author : Pinelia Luna
///  - Update : 2021.06.16
///  - Version : 2.0.0
///  - Contact : pinel0102@gmail.com
/// 
/// ★ Summary
///  - 텍스트 파일의 입출력과 #section:key=value 형식을 사용할 수 있는 클래스입니다.
///  - 기본값으로 앱 외부에 존재하는 [PersistentDataPath] 폴더를 사용합니다.
///  - 에디터 등의 한정된 환경에서 앱 내부에 존재하는 [Resources] 폴더에 쓰기를 사용할 수 있습니다.
///  - 파일 읽고 쓰기시 암호화/복호화를 할 수 있습니다.
///  - #section, key=value 의 형태로 이루어진 파일을 읽고 쓸 수 있습니다.
///  - 한 라인에 하나의 key = value 만 존재할 수 있습니다.
///  - 이 클래스는 유니티 에서만 사용할 수 있습니다.
/// 
/// ★ How to use
///  - FileIO.cs 파일을 프로젝트에 임포트 합니다.
///  - FileIO.MethodName(); 과 같은 방식으로 사용합니다.
///  
/// ★ Text-file Form
///  　#Section1
///  　　Key1 = Value1
///	 　　Key2 = Value2
///  
///  　#Section2
///  　　Key1 = Value1
///  　　Key2 = Value2
/// 
/// ★ Methods
///  - Handle File
///   - public void LoadFile()                 // [PersistentDataPath]/sourcePath 파일의 텍스트 내용을 wholeString 에 저장합니다.
///   - public void SaveFile()                 // wholeString 의 내용을 [PersistentDataPath]/targetPath 파일에 저장합니다.
///   - public void BackupFile()               // [PersistentDataPath]/[bak] 폴더에 [PersistentDataPath]/sourcePath 파일의 백업본을 저장합니다.
///
///  - Handle Value
///   - public [void, string] GetValue()       // wholeString 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 불러옵니다.
///   - public [void, string] SetValue()       // wholeString 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 변경합니다.
///
///  - Handle Form
///   - public [void, string] AddSection()     // wholeString 의 하단에 #thisSection 을 추가합니다.
///   - public [void, string] DelSection()     // #thisSection 과 하단의 모든 key=value 를 삭제합니다.
///   - public [void, string] AddKeyValue()    // #thisSection 의 하단에 thisKey=newValue 형태의 라인을 추가합니다.
///   - public [void, string] DelKeyValue()    // #thisSection 의 하단에 thisKey=newValue 형태의 라인을 삭제합니다.
///   - public bool SectionIsExist()           // wholeString 에 #thisSection 이 존재하는지 확인합니다.
///   - public bool KeyIsExist()               // #thisSection 의 하단에 thisKey 가 존재하는지 확인합니다.
///
///  - Utility
///   - private void AnalyzeFolder()           // targetPath 에 폴더가 포함되어 있을 경우 [PersistentDataPath] 에 해당 폴더를 생성합니다.
///   - private bool IsNullOrWhiteSpace()      // string 이 null 또는 공백만으로 되어있는지 검사합니다.
///   - private string ReadFromResources()     // [Resources]/referencePath 파일의 텍스트 내용을 string으로 불러옵니다.
///   - private string EncryptString()         // ClearText 의 내용을 암호화한 string 을 리턴합니다.
///   - private string DecryptString()         // EncryptedText 의 내용을 복호화한 string 을 리턴합니다.
/// 

#endregion ★ Introduction


#region ★ Namespaces

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Cryptography;

#endregion ★ Namespaces


#region ★ Settings

public static class FILEIO_SETTINGS
{
    /// <summary>
    /// <para>암호화/복호화 변경시 이 항목을 직접 변경하세요.</para>
    /// <para>PersistentDataPath 사용시 이 설정과 관계 없이 강제 암호화됩니다.</para>
    /// </summary>
    public const bool useEncrypt = false;

    /// <summary>
    /// 백업 복구 모드 변경시 이 항목을 직접 변경하세요.
    /// </summary>
    public const bool useRecoveryMode = false;

    /// <summary>
    /// 백업전 파일 검사를 위한 #section - key=value의 조합.
    /// <para>sourcePath 파일 안에 동일한 section, key, value가 있어야 합니다.</para>
    /// </summary>
    public readonly static string[] RecoveryModeCheck = new string[3] { "CRCDATA", "CRCDATA", "7340" };

    /// <summary>
    /// 줄바꿈 기호. 
    /// <para>한 라인에 하나의 기호만 존재할 수 있습니다. </para>
    /// <para>파일 생성시 줄바꿈 기호를 0번 index로 생성합니다.</para>
    /// <para>Mac : "\n" 추천</para>
    /// <para>Windows : "\r\n" 추천</para>
    /// </summary>
    public readonly static string[] LineFeedSeperator = new string[2] { "\n", "\r\n" };
}

#endregion ★ Settings


#region ★ Enums

public enum FILEIO_PATH
{		
    PERSISTENT_DATAPATH = 0,
    STREAMING_ASSETS = 1,	
    RESOURCES = 2
}

#endregion ★ Enums


#region ★ Registry Class

/// <summary>
/// <para>──────────────────────────────</para>
/// <para>FileIO 클래스 사용을 보조하는 클래스입니다.</para>
/// <para>파일의 텍스트 내용을 담은 wholeString 을 new Registry() 에서 관리합니다.</para>
/// <para>지원 타입 : string, int, float, bool, enum</para>
/// <para>자세한 사용 방법은 <see cref="FileIO"/> 클래스를 참조하면 됩니다.</para>
/// <para>이 클래스는 유니티 에서만 사용할 수 있습니다.</para>
/// <para>──────────────────────────────</para>
/// </summary>
public class Registry
{
    #region ★ Variables

    /// <summary>
    /// Registry 의 텍스트 내용입니다. (private-working).
    /// </summary>
    private string reg = null;

    /// <summary>
    /// Registry 의 텍스트 내용입니다. (read-only).
    /// </summary>
    public string wholeString { get { return reg; } }

    #endregion ★ Variables


    #region ★ Methods (Constructor)

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>새로운 Registry 를 만듭니다.</para>
    /// <para>sourcePath 를 입력하면 파일의 텍스트 내용을 자동으로 새로운 Registry 에 저장합니다.</para>
    /// <para>sourcePath 파일이 없으면 [Resources]/referencePath 파일을 sourcePath 로 복사 후 재시도합니다.</para>
    /// <para>지원하는 referencePath 의 확장자는 txt/html/htm/xml/bytes/json/csv/yaml/fnt 입니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <example>
    ///		Registry reg = new Registry(sourchPath); → LoadFileInPersistentDataPath(sourchPath)
    ///		Registry reg = new Registry(sourchPath, referencePath); → LoadFileInPersistentDataPath(sourchPath, referencePath)
    ///		Registry reg = new Registry(sourcePath, null, FILEIO_PATH.RESOURCES); → LoadFileInResources(sourcePath)
    /// </example>
    /// <param name="sourcePath">불러을 파일의 경로.</param>
    /// <param name="referencePath">sourcePath 파일이 없을 경우 참조할 txt 파일의 [Resources] 경로.</param>
    /// <param name="pathFlag">폴더 종류. (Resources : referencePath = null)</param>
    public Registry(string sourcePath = null, string referencePath = null, FILEIO_PATH pathFlag = FILEIO_PATH.PERSISTENT_DATAPATH)
    {
        if (!string.IsNullOrEmpty(sourcePath))
        {
            switch(pathFlag)
            {
                case FILEIO_PATH.PERSISTENT_DATAPATH:
                    LoadFileInPersistentDataPath(sourcePath, referencePath);
                    break;
                case FILEIO_PATH.STREAMING_ASSETS:
                    LoadFileInStreamingAssets(sourcePath, referencePath);
                    break;
                case FILEIO_PATH.RESOURCES:
                    LoadFileInResources(sourcePath);
                    break;
            }
        }
    }

    #endregion ★ Methods (Constructor)


    #region ★ Methods (Handle File)

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>[PersistentDataPath]/sourcePath 파일의 텍스트 내용을 Registry 에 저장합니다.</para>
    /// <para>sourcePath 파일이 없으면 [Resources]/referencePath 파일을 sourcePath 로 복사 후 재시도합니다.</para>
    /// <para>지원하는 referencePath 의 확장자는 txt/html/htm/xml/bytes/json/csv/yaml/fnt 입니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="sourcePath">불러을 파일의 경로.</param>
    /// <param name="referencePath">sourcePath 파일이 없을 경우 참조할 txt 파일의 [Resources] 경로.</param>
    public void LoadFileInPersistentDataPath(string sourcePath, string referencePath = null)
    {
        FileIO.LoadFile(ref reg, sourcePath, referencePath, FILEIO_PATH.PERSISTENT_DATAPATH);
    }	

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>[StreamingAssets]/sourcePath 파일의 텍스트 내용을 Registry 에 저장합니다.</para>
    /// <para>sourcePath 파일이 없으면 [Resources]/referencePath 파일을 sourcePath 로 복사 후 재시도합니다.</para>
    /// <para>지원하는 referencePath 의 확장자는 txt/html/htm/xml/bytes/json/csv/yaml/fnt 입니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="sourcePath">불러을 파일의 경로.</param>
    /// <param name="referencePath">sourcePath 파일이 없을 경우 참조할 txt 파일의 [Resources] 경로.</param>
    public void LoadFileInStreamingAssets(string sourcePath, string referencePath = null)
    {
        FileIO.LoadFile(ref reg, sourcePath, referencePath, FILEIO_PATH.STREAMING_ASSETS);
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>[Resources]/sourcePath 파일의 텍스트 내용을 Registry 에 저장합니다.</para>
    /// <para>sourcePath 파일은 반드시 있어야 합니다.</para>
    /// <para>지원하는 sourcePath 의 확장자는 txt/html/htm/xml/bytes/json/csv/yaml/fnt 입니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="sourcePath">불러을 파일의 경로.</param>
    public void LoadFileInResources(string sourcePath)
    {
        FileIO.LoadFile(ref reg, sourcePath, null, FILEIO_PATH.RESOURCES);
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 의 내용을 [PersistentDataPath]/targetPath 파일에 저장합니다.</para>
    /// <para>targetPath 파일의 기존 내용은 모두 지워집니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="targetPath">저장할 파일의 경로.</param>
    /// <param name="pathFlag">폴더 종류.</param>
    public void SaveFile(string targetPath, FILEIO_PATH pathFlag = FILEIO_PATH.PERSISTENT_DATAPATH)
    {
        switch(pathFlag)
        {
            case FILEIO_PATH.PERSISTENT_DATAPATH:
                SaveFileInPersistentDataPath(targetPath);
                break;
            case FILEIO_PATH.STREAMING_ASSETS:
                SaveFileInStreamingAssets(targetPath);
                break;
            case FILEIO_PATH.RESOURCES:
                SaveFileInResources(targetPath);
                break;
        }
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 의 내용을 [PersistentDataPath]/targetPath 파일에 저장합니다.</para>
    /// <para>targetPath 파일의 기존 내용은 모두 지워집니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="targetPath">저장할 파일의 경로.</param>
    public void SaveFileInPersistentDataPath(string targetPath)
    {
        FileIO.SaveFile(reg, targetPath, FILEIO_PATH.PERSISTENT_DATAPATH);
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 의 내용을 [StreamingAssets]/targetPath 파일에 저장합니다.</para>
    /// <para>targetPath 파일의 기존 내용은 모두 지워집니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="targetPath">저장할 파일의 경로.</param>
    public void SaveFileInStreamingAssets(string targetPath)
    {
        FileIO.SaveFile(reg, targetPath, FILEIO_PATH.STREAMING_ASSETS);
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 의 내용을 [Resources]/targetPath 파일에 저장합니다.</para>
    /// <para>targetPath 파일의 기존 내용은 모두 지워집니다.</para>
    /// <para>유니티 에디터 전용입니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="targetPath">저장할 파일의 경로.</param>
    public void SaveFileInResources(string targetPath)
    {
        FileIO.SaveFile(reg, targetPath, FILEIO_PATH.RESOURCES);
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>[PersistentDataPath]/[bak] 폴더에 [PersistentDataPath]/sourcePath 파일의 백업본을 저장합니다.</para>
    /// <para>[PersistentDataPath]는 디바이스가 제공하는 읽기/쓰기용 고유 경로입니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="sourcePath">백업할 파일의 경로.</param>
    public void BackupFIle(string sourcePath)
    {
        FileIO.BackupFile(sourcePath);
    }

    #endregion ★ Methods (Handle File)


    #region ★ Methods (Handle Value)

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 변경합니다.</para>
    /// <para>#thisSection 또는 thisKey 가 없으면 새롭게 생성하며 newValue 를 적용합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">찾을 section. (string, float, int, enum)</param>
    /// <param name="thisKey">찾을 key. (string, float, int, enum)</param>
    /// <param name="newValue">새로운 value. (string, float, int, bool, enum)</param>
    public void SetValue(object thisSection, object thisKey, object newValue)
    {
        FileIO.SetValue(ref reg, thisSection.ToString(), thisKey.ToString(), newValue.ToString());
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 불러옵니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">찾을 section. (string, float, int, enum)</param>
    /// <param name="thisKey">찾을 key. (string, float, int, enum)</param>
    /// <param name="defaultValue">해당 key가 존재하지 않을 경우 key 생성과 함께 할당할 값.</param>
    /// <returns>#thisSection 의 하단에 있는 thisKey 의 value. (string)</returns>
    public string GetValue(object thisSection, object thisKey, string defaultValue = "default")
    {
        if (!KeyIsExist(thisSection, thisKey))
            AddKeyValue(thisSection, thisKey, defaultValue);
        return FileIO.GetValue(reg, thisSection.ToString(), thisKey.ToString());
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 불러옵니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">찾을 section. (string, float, int, enum)</param>
    /// <param name="thisKey">찾을 key. (string, float, int, enum)</param>
    /// <param name="defaultValue">해당 key가 존재하지 않을 경우 key 생성과 함께 할당할 값.</param>
    /// <returns>#thisSection 의 하단에 있는 thisKey 의 value. (float)</returns>
    public float GetValueFloat(object thisSection, object thisKey, float defaultValue = 0)
    {
        if (!KeyIsExist(thisSection, thisKey))
            AddKeyValue(thisSection, thisKey, defaultValue);
        return float.Parse(FileIO.GetValue(reg, thisSection.ToString(), thisKey.ToString()));
    }
    
    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 불러옵니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">찾을 section. (string, float, int, enum)</param>
    /// <param name="thisKey">찾을 key. (string, float, int, enum)</param>
    /// <param name="defaultValue">해당 key가 존재하지 않을 경우 key 생성과 함께 할당할 값.</param>
    /// <returns>#thisSection 의 하단에 있는 thisKey 의 value. (int)</returns>
    public int GetValueInt(object thisSection, object thisKey, int defaultValue = 0)
    {
        if (!KeyIsExist(thisSection, thisKey))
            AddKeyValue(thisSection, thisKey, defaultValue);
        return int.Parse(FileIO.GetValue(reg, thisSection.ToString(), thisKey.ToString()));
    }
    
    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 불러옵니다.</para>
    /// <para>(true/false), (1/0) 을 인식하며 인식하지 못하면 false 를 불러옵니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">찾을 section. (string, float, int, enum)</param>
    /// <param name="thisKey">찾을 key. (string, float, int, enum)</param>
    /// <param name="defaultValue">해당 key가 존재하지 않을 경우 key 생성과 함께 할당할 값.</param>
    /// <returns>#thisSection 의 하단에 있는 thisKey 의 value. (bool)</returns>
    public bool GetValueBool(object thisSection, object thisKey, bool defaultValue = false)
    {
        if (!KeyIsExist(thisSection, thisKey)) 
            AddKeyValue(thisSection, thisKey, defaultValue);

        string strValue = null;
        strValue = FileIO.GetValue(reg, thisSection.ToString(), thisKey.ToString());

        if (strValue != null)
        {
            try
            {
                return bool.Parse(strValue);
            }
            catch
            {
                if (strValue.Equals("1"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 불러옵니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">찾을 section. (string, float, int, enum)</param>
    /// <param name="thisKey">찾을 key. (string, float, int, enum)</param>
    /// <param name="defaultValue">해당 key가 존재하지 않을 경우 key 생성과 함께 할당할 값.</param>
    /// <returns>#thisSection 의 하단에 있는 thisKey 의 value. (object : casting 필요)</returns>
    public Enum GetValueEnum(object thisSection, object thisKey, Enum defaultValue)
    {
        if (!KeyIsExist(thisSection, thisKey))
            AddKeyValue(thisSection, thisKey, defaultValue);
        return FileIO.GetEnum(defaultValue.GetType(), FileIO.GetValue(reg, thisSection.ToString(), thisKey.ToString()));
        //return (Enum)Enum.Parse(defaultValue.GetType(), FileIO.GetValue(reg, thisSection.ToString(), thisKey.ToString()));
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 불러옵니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">찾을 section. (string, float, int, enum)</param>
    /// <param name="thisKey">찾을 key. (string, float, int, enum)</param>
    /// <param name="resultValue">value 를 저장할 string. (ref)</param>
    /// <param name="defaultValue">해당 key가 존재하지 않을 경우 key 생성과 함께 할당할 값.</param>
    public void GetValue(object thisSection, object thisKey, ref string resultValue, string defaultValue = "default")
    {
        if (!KeyIsExist(thisSection, thisKey)) 
            AddKeyValue(thisSection, thisKey, defaultValue);			
        FileIO.GetValue(reg, thisSection.ToString(), thisKey.ToString(), ref resultValue);
    }	

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 불러옵니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">찾을 section. (string, float, int, enum)</param>
    /// <param name="thisKey">찾을 key. (string, float, int, enum)</param>
    /// <param name="resultValue">value 를 저장할 float. (ref)</param>
    /// <param name="defaultValue">해당 key가 존재하지 않을 경우 key 생성과 함께 할당할 값.</param>
    public void GetValue(object thisSection, object thisKey, ref float resultValue, float defaultValue = 0)
    {
        if (!KeyIsExist(thisSection, thisKey)) 
            AddKeyValue(thisSection, thisKey, defaultValue);
        string strValue = null;
        FileIO.GetValue(reg, thisSection.ToString(), thisKey.ToString(), ref strValue);
        resultValue = float.Parse(strValue);
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 불러옵니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">찾을 section. (string, float, int, enum)</param>
    /// <param name="thisKey">찾을 key. (string, float, int, enum)</param>
    /// <param name="resultValue">value 를 저장할 int. (ref)</param>
    /// <param name="defaultValue">해당 key가 존재하지 않을 경우 key 생성과 함께 할당할 값.</param>
    public void GetValue(object thisSection, object thisKey, ref int resultValue, int defaultValue = 0)
    {
        if (!KeyIsExist(thisSection, thisKey)) 
            AddKeyValue(thisSection, thisKey, defaultValue);
        string strValue = null;
        FileIO.GetValue(reg, thisSection.ToString(), thisKey.ToString(), ref strValue);
        resultValue = int.Parse(strValue);
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 불러옵니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">찾을 section. (string, float, int, enum)</param>
    /// <param name="thisKey">찾을 key. (string, float, int, enum)</param>
    /// <param name="resultValue">value 를 저장할 long. (ref)</param>
    /// <param name="defaultValue">해당 key가 존재하지 않을 경우 key 생성과 함께 할당할 값.</param>
    public void GetValue(object thisSection, object thisKey, ref long resultValue, long defaultValue = 0)
    {
        if (!KeyIsExist(thisSection, thisKey)) 
            AddKeyValue(thisSection, thisKey, defaultValue);
        string strValue = null;
        FileIO.GetValue(reg, thisSection.ToString(), thisKey.ToString(), ref strValue);
        resultValue = long.Parse(strValue);
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 불러옵니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">찾을 section. (string, float, int, enum)</param>
    /// <param name="thisKey">찾을 key. (string, float, int, enum)</param>
    /// <param name="resultValue">value 를 저장할 double. (ref)</param>
    /// <param name="defaultValue">해당 key가 존재하지 않을 경우 key 생성과 함께 할당할 값.</param>
    public void GetValue(object thisSection, object thisKey, ref double resultValue, long defaultValue = 0)
    {
        if (!KeyIsExist(thisSection, thisKey)) 
            AddKeyValue(thisSection, thisKey, defaultValue);
        string strValue = null;
        FileIO.GetValue(reg, thisSection.ToString(), thisKey.ToString(), ref strValue);
        resultValue = long.Parse(strValue);
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 불러옵니다.</para>
    /// <para>(true/false), (1/0) 을 인식하며 인식하지 못하면 false 를 불러옵니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">찾을 section. (string, float, int, enum)</param>
    /// <param name="thisKey">찾을 key. (string, float, int, enum)</param>
    /// <param name="resultValue">value 를 저장할 bool. (ref)</param>
    /// <param name="defaultValue">해당 key가 존재하지 않을 경우 key 생성과 함께 할당할 값.</param>
    public void GetValue(object thisSection, object thisKey, ref bool resultValue, bool defaultValue = false)
    {
        if (!KeyIsExist(thisSection, thisKey)) 
            AddKeyValue(thisSection, thisKey, defaultValue);
        string strValue = null;
        FileIO.GetValue(reg, thisSection.ToString(), thisKey.ToString(), ref strValue);

        if (strValue != null)
        {
            try
            {
                resultValue = bool.Parse(strValue);
            }
            catch
            {
                if (strValue.Equals("1"))
                {
                    resultValue = true;
                }
                else
                {
                    resultValue = false;
                }
            }
        }
        else
        {
            resultValue = false;
        }
    }

    public void ClearDataToNULL()
    {
        reg = null;
    }

    #endregion ★ Methods (Handle Value)


    #region ★ Methods (Handle Form)

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 의 하단에 #thisSection 을 추가합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">추가할 section. (string, float, int, enum)</param>
    public void AddSection(object thisSection)
    {
        FileIO.AddSection(ref reg, thisSection.ToString());
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>#thisSection 과 하단의 모든 key=value 를 삭제합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">삭제할 section. (string, float, int, enum)</param>
    /// <param name="deleteKeyValueOnly">section은 남기고 key=value들만 삭제.</param>	
    public void DelSection(object thisSection, bool deletekeyValueOnly = false)
    {
        FileIO.DelSection(ref reg, thisSection.ToString(), deletekeyValueOnly);
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>#thisSection 의 하단에 thisKey=newValue 형태의 라인을 추가합니다.</para>
    /// <para>#thisSection 이 없으면 wholeString 의 하단에 #thisSection 을 생성합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">찾을 section. (string, float, int, enum)</param>
    /// <param name="thisKey">추가할 key. (string, float, int, enum)</param>
    /// <param name="newValue">추가할 value. (string, float, int, bool, enum)</param>
    public void AddKeyValue(object thisSection, object thisKey, object newValue)
    {
        FileIO.AddKeyValue(ref reg, thisSection.ToString(), thisKey.ToString(), newValue.ToString());
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>#thisSection 의 하단에 thisKey=newValue 형태의 라인을 삭제합니다.</para>
    /// <para>#thisSection 의 하단에 key가 모두 삭제되어도 #thisSection 이 삭제되지는 않습니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">찾을 section. (string, float, int, enum)</param>
    /// <param name="thisKey">삭제할 key. (string, float, int, enum)</param>
    public void DelKeyValue(object thisSection, object thisKey)
    {
        FileIO.DelKeyValue(ref reg, thisSection.ToString(), thisKey.ToString());
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>Registry 에 #thisSection 이 존재하는지 확인합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">확인할 section. (string, float, int, enum)</param>
    /// <returns>#thisSection 존재 여부.</returns>
    public bool SectionIsExist(object thisSection)
    {
        return FileIO.SectionIsExist(reg, thisSection.ToString());
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>#thisSection 의 하단에 thisKey 가 존재하는지 확인합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="thisSection">확인할 section. (string, float, int, enum)</param>
    /// <param name="thisKey">확인할 key. (string, float, int, enum)</param>
    /// <returns>thisKey 존재 여부.</returns>
    public bool KeyIsExist(object thisSection, object thisKey)
    {
        return FileIO.KeyIsExist(reg, thisSection.ToString(), thisKey.ToString());
    }


    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>#thisSection 의 하단에 key=value 페어가 몇 개 존재하는지 확인합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>	
    /// <param name="thisSection">확인할 section. (string, float, int, enum)</param>
    /// <returns>#thisSection 의 key=value 페어 개수.</returns>
    public int GetCountKeyValuePair(object thisSection)
    {
        return FileIO.GetCountKeyValuePair(reg, thisSection.ToString());
    }

    #endregion ★ Methods (Handle Form)
}

#endregion ★ Registry Class



public static class FileIO
{
    public static FilePath Application = new FilePath
    {
        dataPath = UnityEngine.Application.dataPath,
        persistentDataPath = $"{UnityEngine.Application.persistentDataPath}/{UnityEngine.Application.identifier}",
        streamingAssetsPath = UnityEngine.Application.streamingAssetsPath
    };

    #region ★ Variables

    /// <summary>
    /// 암호화/복호화 여부.
    /// </summary>
    private static bool useEncrypt = FILEIO_SETTINGS.useEncrypt;

    /// <summary>
    /// <see cref="SetValue(ref string, string, string, string)"/> 실행시 #section 이나 key 가 없을 때 자동으로 추가합니다.
    /// </summary>
    private static bool useAutoAddingMode = true;

    /// <summary>
    /// 복구 모드 사용 여부.
    /// <para>이 값이 true 일 경우 <see cref="BackupFile(string)"/> 실행시 sourcePath 파일에 이상이 있으면 백업본으로부터 복구를 시도합니다.</para>
    /// <para>해당 옵션을 사용하려면 sourcePath 파일 안에 <see cref="RecoveryModeCheck"/> 와 동일한 #section - key=value가 있어야 합니다.</para>
    /// </summary>
    private static bool useRecoveryMode = FILEIO_SETTINGS.useRecoveryMode;

    /// <summary>
    /// [Resources] 폴더에서 로드할 수 있는 파일 형식.
    /// <para>{ ".txt", ".html", ".htm", ".xml", ".bytes", ".json", ".csv", ".yaml", ".fnt" }</para>
    /// </summary>
    private static string[] SupportExtensions = new string[] { ".txt", ".html", ".htm", ".xml", ".bytes", ".json", ".csv", ".yaml", ".fnt" };

    /// <summary>
    /// 백업전 파일 검사를 위한 #section - key=value의 조합.
    /// <para>sourcePath 파일 안에 동일한 section, key, value가 있어야 합니다.</para>
    /// </summary>
    private static string[] RecoveryModeCheck = FILEIO_SETTINGS.RecoveryModeCheck;

    /// <summary>
    /// section 을 시작하는 기호. 
    /// <para>한 라인에 하나의 기호만 존재할 수 있습니다.</para>
    /// <para>{ "#" }</para>
    /// </summary>
    private static string[] SectionSeperator = new string[] { "#" };

    /// <summary>
    /// key 와 value 를 구분하는 기호. 
    /// <para>한 라인에 복수의 기호가 존재할 수 있습니다. [0]으로 작동.</para>
    /// <para>{ "=", "\t", "\t:\t" }</para>
    /// </summary>
    private static string[] KeyValueSeperator = new string[] { "=", "\t", "\t:\t" };

    /// <summary>
    /// 줄바꿈 기호. 
    /// <para>한 라인에 하나의 기호만 존재할 수 있습니다. </para>
    /// <para>{ "\r\n", "\n" }</para>
    /// </summary>
    private static string[] LineFeedSeperator = FILEIO_SETTINGS.LineFeedSeperator;

    /// <summary>
    /// 암호화/복호화에 사용되는 키 1. (16)
    /// </summary>
    private static string KEY_1 = @"ryojvlzmdalyglrj";

    /// <summary>
    /// 암호화/복호화에 사용되는 키 2. (32)
    /// </summary>
    private static string KEY_2 = @"hcxilkqbbhczfeultgbskdmaunivmfuo";

    /// <summary>
    /// 존재하지 않는 #Section - Key 조합에 <see cref="GetValue(string, string, string, ref string)"/> 를 시도했을 때 출력.
    /// </summary>
    public static string NOT_FOUND_KEY = "not found key";


    public struct FilePath
    {
        public string dataPath;
        public string persistentDataPath;
        public string streamingAssetsPath;		
    }
    
    #endregion


    #region ★ Methods (Handle File)


    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>[pathFlag]/sourcePath 파일의 텍스트 내용을 wholeString 에 저장합니다.</para>	
    /// <para>sourcePath 파일이 없으면 [Resources]/referencePath 파일을 sourcePath 로 복사 후 재시도합니다. </para>
    /// <para>지원하는 referencePath 의 확장자는 txt/html/htm/xml/bytes/json/csv/yaml/fnt 입니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <example>
    ///		string wholeString = null;
    ///		LoadFile(ref wholeString, "Save/UserData.txt", "Home/UserData.txt");
    ///		
    ///		[pathFlag]/Save/UserData.txt 파일이 있으면 내용을 불러옵니다.
    /// 	[pathFlag]/Save/UserData.txt 파일이 없으면 [Resources]/Home/UserData.txt 를 [pathFlag]/Save/UserData.txt 로 복사하고 내용을 불러옵니다.
    /// </example>
    /// <param name="wholeString">파일의 내용을 저장할 string. (ref)</param>
    /// <param name="sourcePath">불러을 파일의 경로.</param>
    /// <param name="referencePath">sourcePath 파일이 없을 경우 참조할 txt 파일의 [Resources] 경로.</param>
    /// <param name="pathFlag">불러올 파일의 위치 지정.</param>
    public static void LoadFile(ref string wholeString, string sourcePath, string referencePath = null, FILEIO_PATH pathFlag = FILEIO_PATH.PERSISTENT_DATAPATH)
    {
        try
        {
            if (IsNullOrWhiteSpace(sourcePath))
            {
                UnityEngine.Debug.Log(CodeManager.GetMethodName() + "sourcePath is empty (" + sourcePath + ")");
                return;
            }

            // sourcePath 는 앞에 "/"가 있어야 함.
            if (!sourcePath.StartsWith("/"))
                sourcePath = "/" + sourcePath;

            int error = 0;

            RESTART:

            FileInfo theSourceFile = null;
            StreamReader sr = null;
            string roughData = null;
            string newData = null;

            switch (pathFlag)
            {
                case FILEIO_PATH.PERSISTENT_DATAPATH:	
                    // [PersistentDataPath]/sourcePath 파일을 로드.

                    //UnityEngine.Debug.Log(CodeManager.GetMethodName() + "[persistentDataPath]" + sourcePath);

                    theSourceFile = new FileInfo(Application.persistentDataPath + sourcePath);

                    if (theSourceFile != null && theSourceFile.Exists)
                    {
                        // 강제 갱신.
                        //File.Delete(Application.persistentDataPath + sourcePath);
                        //goto RESTART;

                        sr = theSourceFile.OpenText();
                    }
                    else if (!IsNullOrWhiteSpace(referencePath))
                    {
                        // [Resources]/referencePath 파일을 로드.

                        UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Create File : [persistentDataPath]" + sourcePath);

                        AnalyzeFolder(sourcePath);

                        //파일 생성
                        FileStream file = File.Create(Application.persistentDataPath + sourcePath);
                        file.Close();

                        //디폴트 파일 읽기 (Read-only)
                        newData = ReadFromResources(referencePath);

                        //파일 쓰기
                        SaveFile(newData, sourcePath, FILEIO_PATH.PERSISTENT_DATAPATH);

                        error++;

                        if (error == 1) goto RESTART;
                    }
                    else
                    {
                        UnityEngine.Debug.Log(CodeManager.GetMethodName() + "sourcePath == NULL && referencePath == NULL");
                    }

                    roughData = sr.ReadToEnd();
                    newData = DecryptString(roughData, true);

                    sr.Close();

                    sourcePath = Application.persistentDataPath + sourcePath;

                    break;

                case FILEIO_PATH.STREAMING_ASSETS:

                    // [StreamingAssets]/sourcePath 파일을 로드.

                    UnityEngine.Debug.Log(CodeManager.GetMethodName() + "[StreamingAssets]" + sourcePath);

                    theSourceFile = new FileInfo(Application.streamingAssetsPath + sourcePath);

                    if (theSourceFile != null && theSourceFile.Exists)
                    {
                        // 강제 갱신.
                        //File.Delete(Application.persistentDataPath + sourcePath);
                        //goto RESTART;

                        sr = theSourceFile.OpenText();
                    }
                    else if (!IsNullOrWhiteSpace(referencePath))
                    {
                        // [Resources]/referencePath 파일을 로드.

                        UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Create File : [StreamingAssets]" + sourcePath);

                        AnalyzeFolder(sourcePath, FILEIO_PATH.STREAMING_ASSETS);

                        //파일 생성
                        FileStream file = File.Create(Application.streamingAssetsPath + sourcePath);
                        file.Close();

                        //디폴트 파일 읽기 (Read-only)
                        newData = ReadFromResources(referencePath);

                        //파일 쓰기
                        SaveFile(newData, sourcePath, FILEIO_PATH.STREAMING_ASSETS);

                        error++;

                        if (error == 1) goto RESTART;
                    }
                    else
                    {
                        bool createEmpty = true;

                        if (createEmpty)
                        {
                            UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Create Empty : [StreamingAssets]" + sourcePath);

                            //파일 생성
                            FileStream file = File.Create(Application.streamingAssetsPath + sourcePath);
                            file.Close();

                            error++;

                            if (error == 1) goto RESTART;
                        }
                        else
                        {
                            UnityEngine.Debug.Log(CodeManager.GetMethodName() + "sourcePath == NULL && referencePath == NULL");
                        }
                    }

                    newData = DecryptString(sr.ReadToEnd());

                    sr.Close();

                    sourcePath = Application.streamingAssetsPath + sourcePath;

                    break;			
                
                case FILEIO_PATH.RESOURCES:
                
                    // [Resources]/sourcePath 파일을 로드.

                    UnityEngine.Debug.Log(CodeManager.GetMethodName() + "[Resources]" + sourcePath);

                    newData = ReadFromResources(sourcePath);

                    break;
            }

            if (IsNullOrWhiteSpace(newData))
            {
                UnityEngine.Debug.Log(CodeManager.GetMethodName() + "The File is Empty : " + sourcePath);
            }
            else
            {
                //UnityEngine.Debug.Log(CodeManager.GetMethodName() + "\n" + roughData);
                //UnityEngine.Debug.Log(CodeManager.GetMethodName() + newData);
                //UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Finished : " + sourcePath);
            }

            wholeString = newData;

            return;

        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Error : " + sourcePath + " : " + e);

            return;
        }
    }


    /// <summary> 
    /// <para>──────────────────────────────</para>
    /// <para>wholeString 의 내용을 [pathFlag]/targetPath 파일에 저장합니다.</para>
    /// <para>targetPath 파일의 기존 내용은 모두 지워집니다.</para>
    /// <para>──────────────────────────────</para>
    ///	<para>　→ wholeString 의 내용이 저장된 targetPath 파일.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>	
    /// <example>	
    ///		SaveFile(wholeString, "Save/UserData.txt");
    ///		
    ///		[pathFlag]/Save/UserData.txt 파일이 있으면 파일의 내용을 모두 지우고 wholeString 의 내용을 저장합니다.
    ///		[pathFlag]/Save/UserData.txt 파일이 없으면 [pathFlag]/Save/UserData.txt 파일을 생성하고 wholeString 의 내용을 저장합니다.
    /// </example>
    /// <param name="wholeString">파일에 저장할 전체 string.</param>
    /// <param name="targetPath">저장할 파일의 경로.</param>
    /// <param name="pathFlag">파일이 저장될 위치 지정.</param>
    public static void SaveFile(string wholeString, string targetPath, FILEIO_PATH pathFlag = FILEIO_PATH.PERSISTENT_DATAPATH)
    {
        try
        {
            if (IsNullOrWhiteSpace(targetPath))
            {
                UnityEngine.Debug.Log(CodeManager.GetMethodName() + "targetPath is empty (" + targetPath + ")");
                return;
            }

            // targetPath 는 앞에 "/"가 있어야 함.
            if (!targetPath.StartsWith("/"))
                targetPath = "/" + targetPath;

            targetPath = targetPath.Replace("\\", "/");

            StreamWriter sw = null;
            string newData = null;

            switch (pathFlag)
            {
                case FILEIO_PATH.PERSISTENT_DATAPATH:

                    // persistentDataPath 폴더에 저장.
                    //UnityEngine.Debug.Log(CodeManager.GetMethodName() + "[persistentDataPath]" + targetPath);

                    AnalyzeFolder(targetPath);

                    targetPath = Application.persistentDataPath + targetPath;
                    sw = new StreamWriter(targetPath);

                    newData = EncryptString(wholeString, true);

                    UnityEngine.Debug.Log(CodeManager.GetMethodName() + wholeString);
                    //UnityEngine.Debug.Log(CodeManager.GetMethodName() + "\n" + newData);

                    sw.Write(newData); // 줄단위로 파일에 입력.
                    sw.Flush();
                    sw.Close();

                    break;

                case FILEIO_PATH.STREAMING_ASSETS:

                    // StreamingAssets 폴더에 저장.
                    //UnityEngine.Debug.Log(CodeManager.GetMethodName() + "[StreamingAssets]" + targetPath);

                    AnalyzeFolder(targetPath, FILEIO_PATH.STREAMING_ASSETS);

                    targetPath = Application.streamingAssetsPath + targetPath;
                    sw = new StreamWriter(targetPath);

                    newData = EncryptString(wholeString);

                    UnityEngine.Debug.Log(CodeManager.GetMethodName() + wholeString);
                    //UnityEngine.Debug.Log(CodeManager.GetMethodName() + "\n" + newData);

                    sw.Write(newData); // 줄단위로 파일에 입력.
                    sw.Flush();
                    sw.Close();

#if UNITY_EDITOR
                    //UnityEngine.Debug.Log("[FileIO] SaveFile : Unity Editor : Refreshing Files");
                    UnityEditor.AssetDatabase.Refresh();
#endif
                    break;

                case FILEIO_PATH.RESOURCES:

                    // Resources 폴더에 저장.
                    UnityEngine.Debug.Log(CodeManager.GetMethodName() + "[Resources] " + targetPath);

                    AnalyzeFolder(targetPath, FILEIO_PATH.RESOURCES);

                    targetPath = Application.dataPath + "/Resources" + targetPath;
                    File.WriteAllText(targetPath, wholeString);

#if UNITY_EDITOR
                    //UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Unity Editor : Refreshing Files");
                    UnityEditor.AssetDatabase.Refresh();
#endif
                    break;
            }			

            UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Finished : " + targetPath);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Error : " + targetPath + " : " + e);
        }
    }


    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>[PersistentDataPath]/[bak] 폴더에 [PersistentDataPath]/sourcePath 파일의 백업본을 저장합니다.</para>
    /// <para>[PersistentDataPath]는 디바이스가 제공하는 읽기/쓰기용 고유 경로입니다.</para>
    /// <para>──────────────────────────────</para>
    ///	<para>　→ sourcePath 의 내용이 저장된 sourcePath.bak 파일.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>	
    /// <example>	
    ///		BackupFile("Save/UserData.txt");
    ///		
    ///		[PersistentDataPath]/Save/UserData.txt 파일의 복사본을 [PersistentDataPath]/[bak]/Save/UserData.txt.bak 으로 생성합니다.
    /// </example>
    /// <param name="sourcePath">백업할 파일의 경로.</param>
    public static void BackupFile(string sourcePath)
    {
        try
        {
            if (IsNullOrWhiteSpace(sourcePath))
            {
                UnityEngine.Debug.Log(CodeManager.GetMethodName() + "sourcePath is empty (" + sourcePath + ")");
                return;
            }

            UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Backup : " + sourcePath);

            string backupFolder = "/bak";
            string backupExt = ".bak";

            if (!sourcePath.StartsWith("/"))
                sourcePath = "/" + sourcePath;

            StringBuilder backupPath = new StringBuilder();
            backupPath.Append(backupFolder);
            backupPath.Append(sourcePath);
            backupPath.Append(backupExt);

            int restart_count = 0;

            string wholeString = null;


            RESTART:

            int first_load_error = 0;

            // 복구 모드 적용시 사용.
            if (useRecoveryMode)
            {
                wholeString = null;// = LoadFile(sourcePath, null, true);
                LoadFile(ref wholeString, sourcePath, null, FILEIO_PATH.PERSISTENT_DATAPATH);

                // 임의의 키 값을 불러 원하는 값과 비교합니다.
                // sourcePath 파일 안에 체크를 위한 section, key, value가 있어야 합니다.
                if (GetValue(wholeString, RecoveryModeCheck[0], RecoveryModeCheck[1]) != RecoveryModeCheck[2])
                    first_load_error++;
            }

            if (first_load_error == 0) // 문제가 없으면 백업본 생성.
            {
                if (!Directory.Exists(Application.persistentDataPath + backupFolder))
                {
                    UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Create Directory for Backup : " + backupPath.ToString());
                    AnalyzeFolder(backupFolder + sourcePath);
                }

                File.Copy(Application.persistentDataPath + sourcePath, Application.persistentDataPath + backupPath.ToString(), true);

                FileInfo theBackupFile = null;
                theBackupFile = new FileInfo(Application.persistentDataPath + backupPath.ToString());

                if (theBackupFile != null && theBackupFile.Exists)
                { UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Finished : " + sourcePath + " → " + backupPath.ToString()); }
                else
                { UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Failed : " + restart_count); }
            }
            else if (restart_count == 0) // 문제가 있으면 백업본으로 복구. 
            {
                backupPath.Remove(0, backupPath.Length);
                backupPath.Append(sourcePath);

                StringBuilder tempstr = new StringBuilder();
                tempstr.Append(backupFolder);
                tempstr.Append(sourcePath);
                tempstr.Append(backupExt);

                sourcePath = null;
                sourcePath = tempstr.ToString();

                restart_count++;

                UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Recovery Start : " + sourcePath + " → " + backupPath.ToString());

                goto RESTART;
            }
            else
            {
                // 백업본에도 문제가 있다.
                UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Failed : " + restart_count);

                //File.Delete(sourcePath);
                //File.Delete(backupPath.ToString());
            }

            backupPath = null;
            wholeString = null;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Failed : " + sourcePath + " : " + e);
        }

    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>targetPath 파일을 삭제합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="targetPath">삭제할 파일 이름.</param>
    /// <param name="pathFlag">삭제할 파일의 위치 지정.</param>
    public static void DeleteFile(string targetPath, FILEIO_PATH pathFlag = FILEIO_PATH.PERSISTENT_DATAPATH)
    {
        try
        {
            // targetPath 는 앞에 "/"가 있어야 함.
            if (!targetPath.StartsWith("/"))
                targetPath = "/" + targetPath;

            FileInfo theSourceFile = null;

            switch (pathFlag)
            {
                case FILEIO_PATH.PERSISTENT_DATAPATH:

                    // [PersistentDataPath]/targetPath 파일을 삭제.
                    theSourceFile = new FileInfo(Application.persistentDataPath + targetPath);

                    if (theSourceFile.Exists)
                    {
                        theSourceFile.Delete();

                        UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Deleted : " + targetPath);
                    }

                    break;

                case FILEIO_PATH.STREAMING_ASSETS:

                    // [StreamingAssets]/targetPath 파일을 삭제.
                    theSourceFile = new FileInfo(Application.streamingAssetsPath + targetPath);

                    if (theSourceFile.Exists)
                    {
                        theSourceFile.Delete();

                        UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Deleted : " + targetPath);
                    }
#if UNITY_EDITOR
                    UnityEditor.AssetDatabase.Refresh();
#endif
                    break;

                case FILEIO_PATH.RESOURCES:

                    // [Resources]/targetPath 파일을 삭제.
                    theSourceFile = new FileInfo(Application.dataPath + "/Resources" + targetPath);

                    if (theSourceFile.Exists)
                    {
                        theSourceFile.Delete();

                        UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Deleted : " + targetPath);
                    }
#if UNITY_EDITOR
                    UnityEditor.AssetDatabase.Refresh();
#endif
                    break;
            }
        }
        catch(Exception e)
        {
            UnityEngine.Debug.Log(CodeManager.GetMethodName() + "Error : " + targetPath + " : " + e);
        }
    }

    #endregion ★ Methods (Handle File)


    #region ★ Methods (Handle Value)


    /// <summary> 
    /// <para>──────────────────────────────</para>
    ///	<para>wholeString 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 불러옵니다.</para>
    ///	<para>──────────────────────────────</para>
    /// </summary>
    /// <example>
    /// 	#options
    /// 	　play_count = 100
    /// 	
    /// 	string playCount = "0";
    /// 	GetValue(wholeString, "options", "play_count", ref playCount);
    /// 	
    /// 	playCount : "100"
    /// </example>
    /// <param name="wholeString">value 를 불러올 전체 string.</param>
    /// <param name="thisSection">찾을 section.</param>
    /// <param name="thisKey">찾을 key.</param>
    /// <param name="newValue">찾은 value 를 저장할 string. (ref)</param>
    public static void GetValue(string wholeString, string thisSection, string thisKey, ref string newValue)
    {
        int load_error = 0;

        try
        {
            if (IsNullOrWhiteSpace(thisSection))
            {
                UnityEngine.Debug.Log("[FileIO] GetValue : thisSection is empty (" + thisSection + ")");
                return;// null;
            }
            if (IsNullOrWhiteSpace(thisKey))
            {
                UnityEngine.Debug.Log("[FileIO] GetValue : thisKey is empty (" + thisKey + ")");
                return;// null;
            }

            bool found = false;

            string[] arr_section = wholeString.Split(SectionSeperator, StringSplitOptions.None);

            for (int i = 1; i < arr_section.GetLength(0); i++)
            {
                string[] arr_type = arr_section[i].Split(LineFeedSeperator, StringSplitOptions.None);

                if (arr_type.Length > 0)
                {
                    if (arr_type[0].Trim().Equals(thisSection, StringComparison.OrdinalIgnoreCase))
                    {
                        for (int j = 1; j < arr_type.GetLength(0); j++)
                        {
                            string[] arr_value = arr_type[j].Split(KeyValueSeperator, StringSplitOptions.RemoveEmptyEntries);

                            if (arr_value.Length > 0)
                            {
                                //UnityEngine.Debug.Log("arr_value[0]: [" + arr_value[0] + "] / thisKey: [" + thisKey + "]");

                                if (arr_value[0].Equals(thisKey, StringComparison.OrdinalIgnoreCase))
                                {
                                    newValue = arr_value[1];
                                    found = true;

                                    break;
                                }
                            }
                        }

                        break;
                    }
                }
            }

            //UnityEngine.Debug.Log("newValue = " + newValue);			

            if (!found)
            {
                load_error++;
                UnityEngine.Debug.Log("[FileIO] GetValue : Failed to find the key (" + thisSection + ", " + thisKey + ") : " + load_error);
            }
        }
        catch (Exception e)
        {
            load_error++;
            UnityEngine.Debug.Log("[FileIO] GetValue : Get Value Failed (" + thisSection + ", " + thisKey + ") : " + load_error + " : " + e);
        }
    }


    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>wholeString 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 변경합니다.</para>
    /// <para>#thisSection 또는 thisKey 가 없으면 새롭게 생성하며 newValue 를 적용합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>		
    /// <example>
    ///		#options
    ///		　play_count = 100
    ///		
    ///		SetValue(ref wholeString, "options", "play_count", "101");
    ///		
    ///		#options
    ///		　play_count = 101	
    /// </example>	
    /// <param name="wholeString">value 를 변경할 전체 string. (ref)</param>
    /// <param name="thisSection">찾을 section.</param>
    /// <param name="thisKey">찾을 key.</param>
    /// <param name="newValue">새로운 value.</param>		
    public static void SetValue(ref string wholeString, string thisSection, string thisKey, string newValue)
    {
        int save_error = 0;

        try
        {
            if (IsNullOrWhiteSpace(thisSection))
            {
                UnityEngine.Debug.Log("[FileIO] SetValue : thisSection is empty (" + thisSection + ")");
                return;
            }
            if (IsNullOrWhiteSpace(thisKey))
            {
                UnityEngine.Debug.Log("[FileIO] SetValue : thisKey is empty (" + thisKey + ")");
                return;
            }
            if (IsNullOrWhiteSpace(newValue))
            {
                UnityEngine.Debug.Log("[FileIO] SetValue : newValue is empty (" + newValue + ")");
                return;
            }


            bool sectionFound = false;
            bool keyFound = false;

            if (String.IsNullOrEmpty(wholeString))
                goto EMPTY;

            string[] arr_section = wholeString.Split(SectionSeperator, StringSplitOptions.None);

            for (int i = 1; i < arr_section.GetLength(0); i++)
            {
                string[] arr_type = arr_section[i].Split(LineFeedSeperator, StringSplitOptions.None);

                if (arr_type.Length > 0)
                {
                    if (arr_type[0].Trim().Equals(thisSection, StringComparison.OrdinalIgnoreCase))
                    {
                        sectionFound = true;

                        for (int j = 1; j < arr_type.GetLength(0); j++)
                        {
                            string[] arr_value = arr_type[j].Split(KeyValueSeperator, StringSplitOptions.RemoveEmptyEntries);

                            if (arr_value.Length > 0)
                            {
                                //UnityEngine.Debug.Log("arr_value[0]: [" + arr_value[0] + "] / thisKey: [" + thisKey + "]");

                                if (arr_value[0].Equals(thisKey, StringComparison.OrdinalIgnoreCase))
                                {
                                    keyFound = true;

                                    arr_value[1] = newValue;
                                    arr_type[j] = string.Join(KeyValueSeperator[0], arr_value);
                                    arr_section[i] = string.Join(LineFeedSeperator[0], arr_type);

                                    wholeString = string.Join(SectionSeperator[0], arr_section);

                                    break;
                                }
                            }
                        }

                        break;
                    }
                }
            }

            EMPTY:

            if (!sectionFound)
            {
                //section 이 없다.
                if (useAutoAddingMode)
                {
                    UnityEngine.Debug.Log("[FileIO] SetValue : Add Section and Key-Value (" + thisSection + ", " + thisKey + ", " + newValue + ")");
                    AddKeyValue(ref wholeString, thisSection, thisKey, newValue);
                }
                else
                {
                    save_error++;
                    UnityEngine.Debug.Log("[FileIO] SetValue : Failed to find the section (" + thisSection + ") : " + save_error);
                }
            }
            else if (!keyFound)
            {
                //section은 있는데 key가 없다.
                if (useAutoAddingMode)
                {
                    //UnityEngine.Debug.Log("[FileIO] SetValue : Add Key-Value (" + thisSection + ", " + thisKey + ", " + newValue + ")");
                    AddKeyValue(ref wholeString, thisSection, thisKey, newValue);
                }
                else
                {
                    save_error++;
                    UnityEngine.Debug.Log("[FileIO] SetValue : Failed to find the key (" + thisSection + ", " + thisKey + ") : " + save_error);
                }
            }
        }
        catch (Exception e)
        {
            save_error++;
            UnityEngine.Debug.Log("[FileIO] SetValue : Set Value Failed (" + thisSection + ", " + thisKey + ") : " + save_error + " :" + e);
        }
    }


    /// <summary>
    /// <para>──────────────────────────────</para>
    ///	<para>wholeString 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 불러옵니다.</para>
    ///	<para>──────────────────────────────</para>
    /// </summary>
    /// <example>
    ///		#options
    ///		　play_count = 100	
    ///		
    ///		string playCount = GetValue(wholeString, "options", "play_count");
    ///		
    ///		playCount : "100"
    ///	</example>
    /// <param name="wholeString">value 를 불러올 전체 string.</param>
    /// <param name="thisSection">찾을 section.</param>
    /// <param name="thisKey">찾을 key.</param>
    /// <returns>#thisSection 의 하단에 있는 thisKey 의 value.</returns>
    public static string GetValue(string wholeString, string thisSection, string thisKey)
    {
        string value = NOT_FOUND_KEY;
        GetValue(wholeString, thisSection, thisKey, ref value);
        return value;
    }


    /// <summary> 
    /// <para>──────────────────────────────</para>
    /// <para>wholeString 에서 #thisSection 의 하단에 있는 thisKey 의 value 를 변경합니다.</para>
    /// <para>#thisSection 또는 thisKey 가 없으면 새롭게 생성하며 newValue 를 적용합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>	
    /// <example>
    ///		#options
    ///		　play_count = 100
    ///		
    ///		wholeString = SetValue(wholeString, "options", "play_count", "101");
    ///		
    ///		#options
    ///		　play_count = 101
    /// </example>	
    /// <param name="wholeString">value 를 변경할 전체 string.</param>
    /// <param name="thisSection">찾을 section.</param>
    /// <param name="thisKey">찾을 key.</param>
    /// <param name="newValue">새로운 value.</param>	
    /// <returns>#thisSection 의 하단에 있는 thisKey 의 value 가 반영된 전체 string.</returns>
    public static string SetValue(string wholeString, string thisSection, string thisKey, string newValue)
    {
        SetValue(ref wholeString, thisSection, thisKey, newValue);
        return wholeString;
    }


    #endregion ★ Methods (Handle Value)


    #region ★ Methods (Handle Form)

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>wholeString 의 하단에 #thisSection 을 추가합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <example>
    ///		#options
    ///		　play_count = 100
    /// 
    ///		AddSection(ref wholeString, "userdata");
    /// 
    ///		#options
    ///		　play_count = 100
    ///		#userdata
    /// </example>
    /// <param name="wholeString">전체 string. (ref)</param>
    /// <param name="thisSection">추가할 section.</param>		
    public static void AddSection(ref string wholeString, string thisSection)
    {
        try
        {
            if (IsNullOrWhiteSpace(thisSection))
            {
                UnityEngine.Debug.Log("[FileIO] AddSection : thisSection is empty (" + thisSection + ")");
                return;
            }

            if (!SectionIsExist(wholeString, thisSection))
            {
                StringBuilder modifiedData = new StringBuilder();
                modifiedData.Append(wholeString);
                modifiedData.Append(LineFeedSeperator[0]);
                modifiedData.Append(SectionSeperator[0]);
                modifiedData.Append(thisSection);
                modifiedData.Append(LineFeedSeperator[0]);

                wholeString = modifiedData.ToString();

                UnityEngine.Debug.Log("[FileIO] AddSection : SUCCESS (" + thisSection + ")");
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("[FileIO] AddSection : Fail (" + thisSection + ") : " + e);
        }
    }


    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>#thisSection 과 하단의 모든 key=value 를 삭제합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <example>
    ///		#options
    ///		　play_count = 100
    ///		#userdata
    ///		　user_num = 300
    ///		
    ///		DelSection(ref wholeString, "userdata");
    ///		
    ///		#options
    ///		　play_count = 100
    /// </example>
    /// <param name="wholeString">전체 string (ref).</param>
    /// <param name="thisSection">삭제할 section.</param>	
    /// <param name="deleteKeyValueOnly">section은 남기고 key=value들만 삭제.</param>	
    public static void DelSection(ref string wholeString, string thisSection, bool deleteKeyValueOnly = false)
    {
        try
        {
            if (SectionIsExist(wholeString, thisSection))
            {
                int found = 0;

                string[] arr_section = wholeString.Split(SectionSeperator, StringSplitOptions.None);
                string[] arr_newSection;

                if (deleteKeyValueOnly)
                    arr_newSection = new string[arr_section.GetLength(0)];
                else
                    arr_newSection = new string[arr_section.GetLength(0) - 1];

                for (int i = 1; i < arr_section.GetLength(0); i++)
                {
                    string[] arr_type = arr_section[i].Split(LineFeedSeperator, StringSplitOptions.None);
                    
                    if (arr_type.Length > 0)
                    {
                        if (deleteKeyValueOnly)
                        {
                            if (arr_type[0].Trim() == thisSection)
                            {
                                //UnityEngine.Debug.Log("[FileIO] DelSection : Found Section to Delete (" + thisSection + ")");

                                found = 1;

                                arr_newSection[i] = string.Format("{0}{1}{1}", thisSection, LineFeedSeperator[0]);								
                            }
                            else
                            {
                                arr_newSection[i] = arr_section[i];
                            }
                        }
                        else
                        {
                            if (arr_type[0].Trim() == thisSection)
                            {
                                UnityEngine.Debug.Log("[FileIO] DelSection : Found Section to Delete (" + thisSection + ")");

                                found = 1;

                                //최하단 section일 경우 이전 section의 줄바꿈 기호 하나를 삭제한다.
                                if (i == arr_section.GetLength(0) - 1)
                                {
                                    if (i == 1) // 유일한 section일 경우 모든 내용을 삭제.
                                    {
                                        UnityEngine.Debug.Log("[FileIO] DelSection : Clear wholeString");
                                        wholeString = null;
                                        return;
                                    }
                                    else
                                        arr_newSection[i - found] = arr_newSection[i - found].Remove(arr_newSection[i - found].Length - LineFeedSeperator[0].Length);
                                }								
                            }
                            else
                            {
                                arr_newSection[i - found] = arr_section[i];
                            }
                        }
                    }
                    
                }

                if (found == 1)
                {
                    if (deleteKeyValueOnly)
                    {
                        //UnityEngine.Debug.Log("[FileIO] DelSection : SUCCESS (" + thisSection + ") - deleteKeyValueOnly");
                    }
                    else
                    {
                        UnityEngine.Debug.Log("[FileIO] DelSection : SUCCESS (" + thisSection + ")");
                    }

                    wholeString = string.Join(SectionSeperator[0], arr_newSection);
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("[FileIO] DelSection : Fail (" + thisSection + ") : " + e);
        }
    }


    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>#thisSection 의 하단에 thisKey=newValue 형태의 라인을 추가합니다.</para>
    /// <para>#thisSection 이 없으면 wholeString 의 하단에 #thisSection 을 생성합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <example>
    ///		#options
    ///		　play_count = 100
    ///		
    ///		AddKeyValue(ref wholeString, "options", "play_time", "3600");
    ///		
    ///		#options
    ///		　play_count = 100
    ///		　play_time = 3600
    /// </example>
    /// <param name="wholeString">전체 string (ref).</param>
    /// <param name="thisSection">찾을 section.</param>
    /// <param name="thisKey">추가할 key.</param>
    /// <param name="newValue">추가할 value.</param>	
    public static void AddKeyValue(ref string wholeString, string thisSection, string thisKey, string newValue)
    {
        try
        {
            if (IsNullOrWhiteSpace(thisSection))
            {
                UnityEngine.Debug.Log("[FileIO] AddKeyValue : thisSection is empty (" + thisSection + ")");
                return;
            }
            if (IsNullOrWhiteSpace(thisKey))
            {
                UnityEngine.Debug.Log("[FileIO] AddKeyValue : thisKey is empty (" + thisKey + ")");
                return;
            }
            if (IsNullOrWhiteSpace(newValue))
            {
                UnityEngine.Debug.Log("[FileIO] AddKeyValue : newValue is empty (" + newValue + ")");
                return;
            }

            //section 이 없으면 추가한다.
            AddSection(ref wholeString, thisSection);

            //key 가 없으면 추가한다.
            if (!KeyIsExist(wholeString, thisSection, thisKey))
            {
                string[] arr_section = wholeString.Split(SectionSeperator, StringSplitOptions.None);

                for (int i = 1; i < arr_section.GetLength(0); i++)
                {
                    string[] arr_type = arr_section[i].Split(LineFeedSeperator, StringSplitOptions.None);

                    if (arr_type.Length > 0)
                    {
                        if (arr_type[0].Trim() == thisSection)
                        {
                            //UnityEngine.Debug.Log("[FileIO] AddKeyValue : Add (" + thisSection + ", " + thisKey + ", " + newValue + ")");

                            StringBuilder modifiedData = new StringBuilder();

                            //줄바꿈 기호가 2개 이상일 경우 줄바꿈 기호 1개를 삭제한다.
                            //UnityEngine.Debug.Log("arr_section[i].LastIndexOf(LineFeedSeperator[0]) : " + arr_section[i].LastIndexOf(LineFeedSeperator[0]));
                            //UnityEngine.Debug.Log("arr_section[i].Length : " + arr_section[i].Length);

                            string taleCut = arr_section[i];
                            string doubleLineFeed = LineFeedSeperator[0] + LineFeedSeperator[0];
                            for (int k = arr_section[i].LastIndexOf(doubleLineFeed); k >= 0; k -= doubleLineFeed.Length)
                            {
                                if (taleCut.Substring(k, doubleLineFeed.Length) == doubleLineFeed)
                                {
                                    taleCut = taleCut.Remove(k, LineFeedSeperator[0].Length);
                                    //UnityEngine.Debug.Log("("+k + ") [" + taleCut + "]");
                                }
                            }

                            modifiedData.Append(taleCut);
                            modifiedData.Append(thisKey);
                            modifiedData.Append(KeyValueSeperator[0]);
                            modifiedData.Append(newValue);
                            modifiedData.Append(LineFeedSeperator[0]);

                            // 마지막 section 이 아닐 경우 줄바꿈 문자를 추가한다.
                            if (i < arr_section.GetLength(0) - 1)
                                modifiedData.Append(LineFeedSeperator[0]);

                            arr_section[i] = modifiedData.ToString();

                            //UnityEngine.Debug.Log(arr_section[i]);

                            wholeString = string.Join(SectionSeperator[0], arr_section);

                            //UnityEngine.Debug.Log("[FileIO] AddKeyValue : SUCCESS (" + thisSection + ", " + thisKey + ", " + newValue + ")");

                            break;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("[FileIO] AddKeyValue : Fail : (" + thisSection + ", " + thisKey + ") : " + e);
        }
    }


    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>#thisSection 의 하단에 thisKey=newValue 형태의 라인을 삭제합니다.</para>
    /// <para>#thisSection 의 하단에 key가 모두 삭제되어도 #thisSection 이 삭제되지는 않습니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <example>
    ///		#options
    ///		　play_count = 100
    ///		　play_time = 3600
    ///		
    ///		DelKeyValue(ref wholeString, "options", "play_count");
    ///		
    ///		#options
    ///		　play_time = 3600
    /// </example>
    /// <param name="wholeString">전체 string (ref).</param>
    /// <param name="thisSection">찾을 section.</param>
    /// <param name="thisKey">삭제할 key.</param>
    public static void DelKeyValue(ref string wholeString, string thisSection, string thisKey)
    {
        try
        {
            if (KeyIsExist(wholeString, thisSection, thisKey))
            {
                int found = 0;

                string[] arr_section = wholeString.Split(SectionSeperator, StringSplitOptions.None);

                for (int i = 1; i < arr_section.GetLength(0); i++)
                {
                    string[] arr_type = arr_section[i].Split(LineFeedSeperator, StringSplitOptions.None);

                    if (arr_type.Length > 0)
                    {
                        if (arr_type[0].Trim() == thisSection)
                        {
                            string[] arr_newType = new string[arr_type.GetLength(0) - 1];

                            //UnityEngine.Debug.Log("arr_type.GetLength(0) : " + arr_type.GetLength(0));
                            for (int j = 0; j < arr_type.GetLength(0); j++)
                            {
                                string[] arr_value = arr_type[j].Split(KeyValueSeperator, StringSplitOptions.RemoveEmptyEntries);
                                //UnityEngine.Debug.Log("arr_value.Length : " + arr_value.Length);

                                if (arr_value.Length > 0)
                                {
                                    if (arr_value[0] == thisKey)
                                    {
                                        found = 1;
                                    }
                                    else
                                    {
                                        arr_newType[j - found] = arr_type[j];
                                    }
                                }
                            }

                            if (found == 1)
                            {
                                arr_section[i] = string.Join(LineFeedSeperator[0], arr_newType);

                                wholeString = string.Join(SectionSeperator[0], arr_section);

                                UnityEngine.Debug.Log("[FileIO] DelKeyValue : SUCCESS (" + thisSection + ", " + thisKey + ")");
                            }

                            break;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("[FileIO] DelKeyValue : Fail : (" + thisSection + ", " + thisKey + ") : " + e);
        }
    }


    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>wholeString 의 하단에 #thisSection 을 추가합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <example>
    ///		#options
    ///		　play_count = 100
    ///		
    ///		wholeString = AddSection(wholeString, "userdata");
    ///		
    ///		#options
    ///		　play_count = 100
    ///		#userdata	
    /// </example>
    /// <param name="wholeString">전체 string.</param>
    /// <param name="thisSection">추가할 section.</param>	
    /// <returns>wholeString 의 하단에 #thisSection 이 추가된 전체 string.</returns>
    public static string AddSection(string wholeString, string thisSection)
    {
        AddSection(ref wholeString, thisSection);
        return wholeString;
    }


    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>#thisSection 과 하단의 모든 key=value 를 삭제합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <example>
    ///		#options
    ///		　play_count = 100
    ///		#userdata
    ///		　user_num = 300
    ///		
    ///		wholeString = DelSection(wholeString, "userdata");
    ///		
    ///		#options
    ///		　play_count = 100
    /// </example>
    /// <param name="wholeString">전체 string.</param>
    /// <param name="thisSection">삭제할 section.</param>	
    /// <returns>#thisSection 과 하단의 모든 key=value 가 삭제된 전체 string.</returns>
    public static string DelSection(string wholeString, string thisSection)
    {
        DelSection(ref wholeString, thisSection);
        return wholeString;
    }


    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>#thisSection 의 하단에 thisKey=newValue 형태의 라인을 추가합니다.</para>
    /// <para>#thisSection 이 없으면 wholeString 의 하단에 #thisSection 을 생성합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <example>
    ///		#options
    ///		　play_count = 100
    ///		
    ///		wholeString = AddKeyValue(wholeString, "options", "play_time", "3600");
    ///		
    ///		#options
    ///		　play_count = 100
    ///		　play_time = 3600
    /// </example>
    /// <param name="wholeString">전체 string.</param>
    /// <param name="thisSection">찾을 section.</param>
    /// <param name="thisKey">추가할 key.</param>
    /// <param name="newValue">추가할 value.</param>
    /// <returns>#thisSection 의 하단에 thisKey=newValue 가 추가된 전체 string.</returns>
    public static string AddKeyValue(string wholeString, string thisSection, string thisKey, string newValue)
    {
        AddKeyValue(ref wholeString, thisSection, thisKey, newValue);
        return wholeString;
    }


    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>#thisSection 의 하단에 thisKey=newValue 형태의 라인을 삭제합니다.</para>
    /// <para>#thisSection 의 하단에 key가 모두 삭제되어도 #thisSection 이 삭제되지는 않습니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <example>
    ///		#options
    ///		　play_count = 100
    ///		　play_time = 3600
    ///		
    ///		wholeString = DelKeyValue(wholeString, "options", "play_count");
    ///		
    ///		#options
    ///		　play_time = 3600
    /// </example>
    /// <param name="wholeString">전체 string.</param>
    /// <param name="thisSection">찾을 section.</param>
    /// <param name="thisKey">삭제할 key.</param>
    /// <returns>#thisSection 의 하단에 thisKey=value 가 삭제된 전체 string.</returns>	
    public static string DelKeyValue(string wholeString, string thisSection, string thisKey)
    {
        DelKeyValue(ref wholeString, thisSection, thisKey);
        return wholeString;
    }


    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>wholeString 에 #thisSection 이 존재하는지 확인합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="wholeString">전체 string.</param>
    /// <param name="thisSection">확인할 section.</param>
    /// <returns>#thisSection 존재 여부.</returns>
    public static bool SectionIsExist(string wholeString, string thisSection)
    {
        try
        {
            if (IsNullOrWhiteSpace(thisSection))
            {
                UnityEngine.Debug.Log("[FileIO] SectionIsExist : thisSection is empty (" + thisSection + ")");
                return false;
            }

            string[] arr_section = wholeString.Split(SectionSeperator, StringSplitOptions.None);

            for (int i = 1; i < arr_section.GetLength(0); i++)
            {
                string[] arr_type = arr_section[i].Split(LineFeedSeperator, StringSplitOptions.None);

                if (arr_type.Length > 0)
                {
                    if (arr_type[0].Trim() == thisSection)
                    {
                        //UnityEngine.Debug.Log("[FileIO] SectionIsExist : thisSection Exists (" + thisSection + ")");

                        return true;
                    }
                }
            }

            return false;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("[FileIO] SectionIsExist : Fail (" + thisSection + ") : " + e);

            return false;
        }
    }


    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>#thisSection 의 하단에 thisKey 가 존재하는지 확인합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="wholeString">전체 string.</param>
    /// <param name="thisSection">확인할 section.</param>
    /// <param name="thisKey">확인할 key.</param>
    /// <returns>thisKey 존재 여부.</returns>
    public static bool KeyIsExist(string wholeString, string thisSection, string thisKey)
    {
        try
        {
            if (IsNullOrWhiteSpace(thisSection))
            {
                UnityEngine.Debug.Log("[FileIO] KeyIsExist : thisSection is empty (" + thisSection + ")");
                return false;
            }
            if (IsNullOrWhiteSpace(thisKey))
            {
                UnityEngine.Debug.Log("[FileIO] KeyIsExist : thisKey is empty (" + thisKey + ")");
                return false;
            }

            string[] arr_section = wholeString.Split(SectionSeperator, StringSplitOptions.None);

            for (int i = 1; i < arr_section.GetLength(0); i++)
            {
                string[] arr_type = arr_section[i].Split(LineFeedSeperator, StringSplitOptions.None);

                if (arr_type.Length > 0)
                {
                    if (arr_type[0].Trim() == thisSection)
                    {
                        for (int j = 1; j < arr_type.GetLength(0); j++)
                        {
                            string[] arr_value = arr_type[j].Split(KeyValueSeperator, StringSplitOptions.RemoveEmptyEntries);

                            if (arr_value.Length > 0)
                            {
                                if (arr_value[0] == thisKey)
                                {
                                    //UnityEngine.Debug.Log("[FileIO] KeyIsExist : thisKey Exists (" + thisSection + ", " + thisKey + ")");

                                    return true;
                                }
                            }
                        }

                        break;
                    }
                }
            }

            return false;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("[FileIO] KeyIsExist : Fail (" + thisSection + ", " + thisKey + ") : " + e);

            return false;
        }
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>#thisSection 의 하단에 key=value 페어가 몇 개 존재하는지 확인합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="wholeString">전체 string.</param>
    /// <param name="thisSection">확인할 section.</param>
    /// <returns>#thisSection 의 key=value 페어 개수.</returns>
    public static int GetCountKeyValuePair(string wholeString, string thisSection)
    {
        if (IsNullOrWhiteSpace(thisSection))
        {
            UnityEngine.Debug.Log("[FileIO] GetCountKeyValuePair : thisSection is empty (" + thisSection + ")");
            return 0;
        }

        int count = 0;

        if (SectionIsExist(wholeString, thisSection))
        {
            string[] arr_section = wholeString.Split(SectionSeperator, StringSplitOptions.None);

            for (int i = 1; i < arr_section.GetLength(0); i++)
            {
                string[] arr_type = arr_section[i].Split(LineFeedSeperator, StringSplitOptions.None);

                if (arr_type.Length > 0)
                {
                    if (arr_type[0].Trim() == thisSection)
                    {
                        for (int j = 1; j < arr_type.GetLength(0); j++)
                        {
                            string[] arr_value = arr_type[j].Split(KeyValueSeperator, StringSplitOptions.RemoveEmptyEntries);

                            if (arr_value.Length > 0)
                            {
                                count++;
                            }
                        }

                        break;
                    }
                }
            }

            if (count > 0)
            {
                //UnityEngine.Debug.Log("[FileIO] GetCountKeyValuePair : SUCCESS (" + thisSection + ", " + count + ")");
            }
        }
        else
        {
            count = 0;

            UnityEngine.Debug.Log("[FileIO] GetCountKeyValuePair : wholeString has no section : " + thisSection);
        }

        return count;
    }

    #endregion ★ Methods (Handle Form)


    #region ★ Methods (Utility)

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>targetPath 에 폴더가 포함되어 있을 경우 해당 폴더를 생성합니다.</para>
    /// <para>하위 폴더를 지원합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="targetPath">체크할 경로.</param>
    public static void AnalyzeFolder(string targetPath, FILEIO_PATH pathflag = FILEIO_PATH.PERSISTENT_DATAPATH)
    {
        targetPath = targetPath.Replace('\\', '/');
        targetPath = targetPath.Replace("//", "/");

        if (!targetPath.StartsWith("/"))
            targetPath = "/" + targetPath;

        string folderPath = string.Empty;

        switch(pathflag)
        {
            case FILEIO_PATH.PERSISTENT_DATAPATH:
                folderPath = Application.persistentDataPath;
                break;
            case FILEIO_PATH.STREAMING_ASSETS:
                folderPath = Application.streamingAssetsPath;
                break;
            case FILEIO_PATH.RESOURCES:
                folderPath = Application.dataPath + "/Resources";
                break;
        }
        
        string fullPath = folderPath + targetPath;
        string fullDirectory = fullPath.Remove(fullPath.LastIndexOf("/"));		

        if (!Directory.Exists(fullDirectory))
        {
            Directory.CreateDirectory(fullDirectory);				
            UnityEngine.Debug.Log("[FileIO] AnalyzeFolder : Create Directory : " + fullDirectory);
        }		
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>[Resources]/referencePath 파일의 텍스트 내용을 string으로 불러옵니다.</para>
    /// <para>referencePath 파일은 반드시 있어야 합니다.</para>
    /// <para>지원하는 referencePath 의 확장자는 txt/html/htm/xml/bytes/json/csv/yaml/fnt 입니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="referencePath">[Assets/Resources] 폴더 안에 있는 파일의 경로.</param>
    /// <returns>[Assets/Resources]/referencePath 파일의 전체 string.</returns>
    private static string ReadFromResources(string referencePath)
    {
        string wholeString = null;

        try
        {
            // referencePath 는 앞에 "/"가 없어야 함.
            if (referencePath.StartsWith("/"))
                referencePath = referencePath.Substring(1);

#if UNITY_EDITOR
            // 유니티 에디터 : 파일이 없으면 생성.
            string fullPath = string.Format("{0}/Resources/{1}", Application.dataPath, referencePath);
            if (!File.Exists(fullPath))
            {
                using (FileStream fs = File.Create(fullPath, 1024))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(string.Empty);
                    fs.Write(info, 0, info.Length);
                }
            }
#endif

            //referencePath 는 확장자가 없어야 함.
            foreach (string ext in SupportExtensions)
            {
                if (referencePath.EndsWith(ext))
                {
                    referencePath = referencePath.Remove(referencePath.Length - ext.Length);
                    break;
                }
            }

            UnityEngine.TextAsset data = (UnityEngine.TextAsset)UnityEngine.Resources.Load(referencePath, typeof(UnityEngine.TextAsset));

            StringReader sr2 = null;
            sr2 = new StringReader(data.text);
            wholeString = sr2.ReadToEnd();
            sr2.Close();

            UnityEngine.Debug.Log("[FileIO] ReadFromResources : Success : " + referencePath);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("[FileIO] ReadFromResources : Fail : " + e);
        }

        return wholeString;
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>ClearText 의 내용을 암호화한 string 을 리턴합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <returns>암호화가 진행된 string.</returns>
    /// <param name="ClearText">암호화를 진행할 string.</param>
    /// <param name="forceEncrypt">useEncrypt == false 일 때 암호화 강제 사용 여부.</param>
    public static string EncryptString(string ClearText, bool forceEncrypt = false)
    {
        if (!useEncrypt && !forceEncrypt)
            return ClearText;

        try
        {
            byte[] clearTextBytes = Encoding.UTF8.GetBytes(ClearText);
            SymmetricAlgorithm rijn = SymmetricAlgorithm.Create();
            MemoryStream ms = new MemoryStream();
            byte[] rgbIV = Encoding.UTF8.GetBytes(KEY_1);
            byte[] key = Encoding.UTF8.GetBytes(KEY_2);
            CryptoStream cs = new CryptoStream(ms, rijn.CreateEncryptor(key, rgbIV), CryptoStreamMode.Write);

            cs.Write(clearTextBytes, 0, clearTextBytes.Length);
            cs.Close();

            return Convert.ToBase64String(ms.ToArray());
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e.ToString());
            return ClearText;
        }
    }


    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>EncryptedText 의 내용을 복호화한 string 을 리턴합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <returns>암호화가 해제된 string.</returns>
    /// <param name="EncryptedText">암호화가 진행된 string.</param>
    /// <param name="forceDecrypt">useEncrypt == false 일 때 복호화 강제 사용 여부.</param>
    public static string DecryptString(string EncryptedText, bool forceDecrypt = false)
    {
        if (!useEncrypt && !forceDecrypt)
            return EncryptedText;

        try
        {
            byte[] encryptedTextBytes = Convert.FromBase64String(EncryptedText);
            SymmetricAlgorithm rijn = SymmetricAlgorithm.Create();
            MemoryStream ms = new MemoryStream();
            byte[] rgbIV = Encoding.UTF8.GetBytes(KEY_1);
            byte[] key = Encoding.UTF8.GetBytes(KEY_2);
            CryptoStream cs = new CryptoStream(ms, rijn.CreateDecryptor(key, rgbIV), CryptoStreamMode.Write);

            cs.Write(encryptedTextBytes, 0, encryptedTextBytes.Length);
            cs.Close();

            return Encoding.UTF8.GetString(ms.ToArray());
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e.ToString());
            return EncryptedText;
        }
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>string 이 null 또는 공백만으로 되어있는지 검사합니다.</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="value">검사할 string.</param>
    /// <returns>null 또는 공백 여부.</returns>
    public static bool IsNullOrWhiteSpace(string value)
    {
        if (value != null)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// <para>──────────────────────────────</para>
    /// <para>object 로부터 Enum 값을 리턴합니다.</para>
    /// <para>리턴값을 (toType) 으로 캐스팅해서 사용할 수 있습니다.</para>
    /// <para>(ENUM_TYPE) FileIO.GetEnum(typeof(ENUM_TYPE), object)</para>
    /// <para>──────────────────────────────</para>
    /// </summary>
    /// <param name="toType">typeof(SomeEnum)</param>
    /// <param name="value">값을 변환할 object.</param>
    /// <returns>Enum Value.</returns>
    public static Enum GetEnum(Type toType, object value)
    {
        return (Enum)Enum.Parse(toType, value.ToString());
    }

    static string SPLIT_RE = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };
    public static List<Dictionary<string, object>> CSVRead(string column, string source)
    {
        var list = new List<Dictionary<string, object>>();
        var header = Regex.Split(column, SPLIT_RE);
        var lines = Regex.Split(source, LINE_SPLIT_RE);        

        for(var i=0; i < lines.Length; i++) 
        {			
            var values = Regex.Split(lines[i], SPLIT_RE);
            if(values.Length == 0 ||values[0] == "") continue;
            
            var entry = new Dictionary<string, object>();
            for(var j=0; j < header.Length && j < values.Length; j++ ) {
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\n", "\n");
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                object finalvalue = value;
                int n;
                float f;
                if(int.TryParse(value, out n)) {
                    finalvalue = n;
                } else if (float.TryParse(value, out f)) {
                    finalvalue = f;
                }
                entry[header[j]] = finalvalue;

                //Debug.Log(CodeManager.GetMethodName() + string.Format("entry[{0}] = {1}", header[j], finalvalue));
            }
            list.Add (entry);			
        }

        return list;
    }

    public static Dictionary<string, object> CSVReadOneLine(string column, string source)
    {
        var header = Regex.Split(column, SPLIT_RE);
        var lines = Regex.Split(source, LINE_SPLIT_RE);					
        var values = Regex.Split(lines[0], SPLIT_RE);

        if(values.Length == 0 || values[0] == "") return null;
        
        var entry = new Dictionary<string, object>();
        for(var j=0; j < header.Length && j < values.Length; j++ ) {
            string value = values[j];
            value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\n", "\n");
            value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
            object finalvalue = value;
            int n;
            float f;
            if(int.TryParse(value, out n)) {
                finalvalue = n;
            } else if (float.TryParse(value, out f)) {
                finalvalue = f;
            }
            entry[header[j]] = finalvalue;

            //Debug.Log(CodeManager.GetMethodName() + string.Format("entry[{0}] = {1}", header[j], finalvalue));
        }
        return entry;
    }

    #endregion ★ Methods (Utility)
}
