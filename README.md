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
#### Program.cs의 _setEnvironment() 메서드를 적절히 수정하십시오.
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

### API 목록
|URL|Method|
|---|---|
|/v1/document/request|POST|
|/v1/document/status|GET|
|/v1/document/result|GET|
- - -
### 새 문서 처리 요청 생성하기
|Method|URL|인증|
|---|---|---|
|POST|/v1/document/request|None|

#### 요청
>[!NOTE]
>Header의 `Content-Type`은 반드시 `multipart/form-data`여야 합니다.

>[!WARNING]
>서버는 개인정보 보호를 위해 요청을 영구적으로 저장하지 않습니다.
>
>**모든 요청은 Redis 데이터베이스에 저장되고, 10분 후에 만료됩니다.**

**Form-data**
|이름|타입|설명|
|---|---|---|
|prompt|string|ChatGPT 처리에 사용할 프롬프트를 지정합니다.|
|fileType|`pdf`, `image`|문서의 타입을 지정합니다.|
|file|stream|문서 파일|

```bash
curl -X POST 'https://localhost:7211/v1/document/request' \
-F 'prompt=summarize the key points of this document in Korean in JSON format.' \
-F 'fileType="pdf" \
-F 'file=@"F:\document\정보통신공학개론_기말시험정리.pdf"'
```

#### 응답
**Json**
|이름|타입|설명|
|---|---|---|
|requestId|string|요청 ID|

```json
{
    "requestId": "b93cdeaf-7e30-4918-8277-4806e3bc6d0c"
}
```
- - -
### 문서 처리 요청의 상태 확인하기
|Method|URL|인증|
|---|---|---|
|GET|/v1/document/status|None|

#### 요청
**URL Parameters**
|이름|타입|설명|
|---|---|---|
|requestId|string|작업의 요청 ID|
```bash
curl -X GET 'https://localhost:7211/v1/document/status?requestId=b93cdeaf-7e30-4918-8277-4806e3bc6d0c'
```
#### 응답
**Json**
|이름|타입|설명|
|---|---|---|
|status|int|작업의 상태|

**status**
|숫자|설명|
|---|---|
|0|작업이 대기 큐에 있음.|
|1|문서를 텍스트로 내보내는 중.|
|2|ChatGPT 응답을 기다리는 중.|
|3|작업이 완료됨.|
|4|작업이 오류로 종료됨.|

```json
{
    "status": 3
}
```
- - -
### 문서 처리 요청의 결과 확인하기
|Method|URL|인증|
|---|---|---|
|GET|/v1/document/result|None|

#### 요청
**URL Parameters**
|requestId|string|작업의 요청 ID|
```bash
curl -X GET 'https://localhost:7211/v1/document/result?requestId=b93cdeaf-7e30-4918-8277-4806e3bc6d0c'
```

#### 응답
**Json**

알 수 없음 - **ChatGPT의 응답이 동적인 형식을 가지고 있음.**

```json
{
  "1": {
    "정보통신망의 개념": {
      "정의": "정보(텍스트, 이미지, 음성 등)를 효율적으로 전송하기 위해 통신장비를 상호 유기적으로 결합한 것",
      "구성": "하나의 회선에 여러 시스템을 연결하거나 몇 개의 회선을 공유하는 방식",
      "목적": "통신 비용 절감 및 정보 효율적 전송"
    },
    "정보통신망의 구성요소": {
      "단말장치": "데이터의 입출력을 수행하는 장치",
      "통신회선": "데이터를 전송하는 통로",
      "교환기": "단말장치 사이에서 효율적인 경로를 설정해주는 역할"
    },
    "정보통신망의 분류": {
      "네트워크 범위와 연결 방식": {
        "LAN": "근거리 통신망",
        "WAN": "광역통신망",
        "MAN": "도시망",
        "VAN": "부가가치망"
      },
      "구성 형태": {
        "트리형": "트리 형태로 연결된 통신망",
        "버스형": "하나의 통신회선에 노드가 분기해서 접속",
        "성형": "중앙 노드를 중심으로 점-대-점 연결",
        "링형": "원형을 이루며 순차적으로 연결된 노드",
        "망형": "기본적인 통신회선망 형태"
      },
      "교환 방식": {
        "회선 교환": "직접 연결하는 방식",
        "축적 교환": {
          "메시지 교환 방식": "메시지 단위로 정보 전송",
          "패킷 교환 방식": "패킷 단위로 정보 전송"
        }
      }
    },
    "정보통신망의 발전 과정": {
      "단계": [
        "단독 시스템",
        "복합 시스템",
        "계층화 시스템",
        "정보통신망의 통합화",
        "인터네트워킹"
      ],
      "인터네트워킹": "근거리 통신망과 광역 통신망 간 상호 접속하여 형성된 광역화된 네트워크 집합"
    }
  }
}
```

## Dependencies
* [cloudinteractive.document](https://github.com/Coppermine-SP/cloudinteractive.document) - MIT License
