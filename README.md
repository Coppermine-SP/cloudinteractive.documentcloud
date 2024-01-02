# cloudinteractive.documentcloud
cloudinteractive.documentcloud는 Microsoft ASP.NET Core를 기반으로 하여 사용자에게 HTTP 요청으로 PDF, Image 형식의 문서를 받아,

이를 [cloudinteractive.document](https://github.com/Coppermine-SP/cloudinteractive.document) 패키지를 사용하여 처리하여 반환하는 API 서버 프로젝트입니다.

>[!WARNING]
> **현재, 이 프로젝트는 Windows에서만 작동 합니다.**
>
> 이는 cloudinteractive.document 패키지의 플렛폼 의존성 문제입니다.
>
> 자세한 사항은 [cloudinteractive.document](https://github.com/Coppermine-SP/cloudinteractive.document) 페이지를 참조하십시오.

### Table of Contents
- [How to Use](#how-to-use)
- [API Documentation](#api-documentation)
- [Dependencies](#dependencies)
  
## How to Use
#### GitHub CLI 또는 GitHub Desktop을 사용해 이 레포지토리를 클론하십시오.
```powershell
gh repo clone CoppermineSP/cloudinteractive.document
```
- - -
#### Program.cs의 _serEnvironment() 메서드를 적절히 수정하십시오.
>[!NOTE]
> `AzureComputerVision.Init()`, `OpenAI.Init()` 메서드가 올바른 API 엔드포인트와 키를 받을 수 있도록 하십시오.
- - -
#### appsettings.json의 `"RedisServer"`가 올바른 Redis 서버를 가르키도록 하십시오.
>[!NOTE]
> Redis 서버는 각 요청의 상태와 결과를 저장하는 것에 사용됩니다.
- - -
#### 프로젝트를 IIS로 배포, 또는 Kestrel을 통해 구동하십시오.
레포지토리의 API Documentation을 참조하여 적절한 HTTP 요청을 보내십시오.
>[!WARNING]
>**이 API 서버는 외부에 노출되는 것을 전제로 설계되지 않았습니다.**
>
>반드시 서버가 게이트웨이 뒤에 있고, 신뢰 할 수 있는 프론트엔드 서버에서 API 요청을 전달하거나, 인트라넷에서만 이 API 서버를 사용해야 합니다.

## API Documentation


## Dependencies
* [cloudinteractive.document](https://github.com/Coppermine-SP/cloudinteractive.document) - MIT License
