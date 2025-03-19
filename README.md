# Unity Custom ShaderGraph-Unlit-With-Shadow

**Unity의 Shader Graph 커스터마이징을 통해 직접 키워드를 정의할 필요 없이 Shadow Node를 사용하는 레포지토리.**  

<details>  
  <summary>English</summary> 
This repository utilizes a Shadow Node through Unity's Custom Shader Graph without the need to manually define keywords.
</details>  

* * *  

## 필요한 이유 / Why it's necessary  
이 레포지토리는 Unity의 Shader Graph를 커스터마이징하여 새로운 그래프를 추가함으로써, Unlit 셰이더에서도 Main Light를 사용 가능하도록 하는 레포지토리입니다.  

유니티의 각종 쉐이더 함수들을 사용하기 위해 Keyword를 직접 정의할 필요 없이 일반 Lit Shader 와 동등하게 키워드를 적용받을 수 있습니다.  

이를 통해 _MAIN_LIGHT_SHADOWS_CASCADE 등의 키워드를 직접 정의할 필요 없이 즉시 Main Light를 사용하는 등의 응용이 가능해집니다.  

<details>  
  <summary>English</summary>  

This repository customizes Unity's Shader Graph by adding a new graph, enabling the use of Main Light fully and seamlessly even with Unlit shaders.  

It allows you to benefit from keywords and various Unity shader functions on par with a standard Lit Shader—without the need to define the keywords manually.  

This means that you can immediately leverage Main Light functionality without having to explicitly define keywords like _MAIN_LIGHT_SHADOWS_CASCADE.  

</details>

## 최신 테스트 버전 / Latest Tested Version
- 테스트 완료 버전 **6000.0.31f URP** (*Latest tested version: **6000.0.31f URP***)

## 시스템 요구 사항 / System Requirements

- Unity **6000.0.9f0** 이상 (*Unity **6000.0.9f0** or later*)
- Shader Graph **17.0.3** 이상 (*Shader Graph **17.0.3** or later*)

## 예제 가이드 / Example Guide

아래 이미지를 통해 셰이더의 적용 예시와 가이드를 확인할 수 있습니다.  
The images below illustrate examples and guides for applying the shader.

![Guide Image 1](./docs/Guide_1.png)  
![Guide Image 2](./docs/Guide_3.png)  
![Guide Image 3](./docs/Guide_2.png)
