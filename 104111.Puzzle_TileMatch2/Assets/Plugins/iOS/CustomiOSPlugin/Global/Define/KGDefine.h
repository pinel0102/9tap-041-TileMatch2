//
//  KGDefine.h
//  Unity-iPhone
//
//  Created by 이상동 on 2020/08/24.
//

#ifndef KGDefine_h
#define KGDefine_h

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <AudioToolbox/AudioToolbox.h>
#import <FBAudienceNetwork/FBAdSettings.h>
#import <AppTrackingTransparency/AppTrackingTransparency.h>

#import "UnityInterface.h"

// 기타
#define EMPTY_STRING			("")

// 결과
#define RESULT_TRUE				("True")
#define RESULT_FALSE			("False")

// 빌드 모드
#define BUILD_MODE_DEBUG			("Debug")
#define BUILD_MODE_RELEASE			("Release")

// 식별자 {
#define ID_KEYCHAIN_DEVICE			("KeychainDeviceID")

#define SYSTEM_SND_ID_LIGHT				(1519)
#define SYSTEM_SND_ID_MEDIUM			(1102)
#define SYSTEM_SND_ID_HEAVY				(1520)
// 식별자 }

// 비율
#define SCALE_ACTIVITY_INDICATOR				(0.25f)
#define SCALE_ACTIVITY_INDICATOR_OFFSET			(0.01f)

// 버전
#define MIN_VERSION_DEVICE_ID_FOR_VENDOR			6.0
#define MIN_VERSION_FEEDBACK_GENERATOR				10.0
#define MIN_VERSION_IMPACT_INTENSITY				13.0
#define MIN_VERSION_ACTIVITY_INDICATOR				13.0

// 명령어
#define CMD_GET_DEVICE_ID				("GetDeviceID")
#define CMD_GET_COUNTRY_CODE			("GetCountryCode")
#define CMD_GET_STORE_VERSION			("GetStoreVersion")
#define CMD_SET_BUILD_MODE				("SetBuildMode")
#define CMD_SHOW_ALERT					("ShowAlert")
#define CMD_VIBRATE						("Vibrate")
#define CMD_ACTIVITY_INDICATOR			("ActivityIndicator")

// 키 {
#define KEY_CMD			("Cmd")
#define KEY_MSG			("Msg")

#define KEY_APP_ID			("AppID")
#define KEY_VERSION			("Version")
#define KEY_TIMEOUT			("Timeout")

#define KEY_ALERT_TITLE						("Title")
#define KEY_ALERT_MSG						("Msg")
#define KEY_ALERT_OK_BTN_TEXT				("OKBtnText")
#define KEY_ALERT_CANCEL_BTN_TEXT			("CancelBtnText")

#define KEY_STORE_VERSION					("version")
#define KEY_STORE_VERSION_RESULT			("results")

#define KEY_VIBRATE_TYPE				("Type")
#define KEY_VIBRATE_STYLE				("Style")
#define KEY_VIBRATE_INTENSITY			("Intensity")

#define KEY_DEVICE_MS_RESULT			("Result")
#define KEY_DEVICE_MS_VERSION			("Version")
// 키 }

// 네트워크 {
#define HTTP_METHOD_GET				("GET")
#define HTTP_METHOD_POST			("POST")

#define URL_FORMAT_STORE_VERSION			("http://itunes.apple.com/lookup?bundleId=%@")
// 네트워크 }

// 이름
#define OBJ_NAME_DEVICE_MSG_RECEIVER				("CDeviceMsgReceiver")
#define FUNC_NAME_DEVICE_MSG_HANDLE_METHOD			("HandleDeviceMsg")

//! 진동 타입
enum class EVibrateType {
	NONE = -1,
	SELECTION,
	NOTIFICATION,
	IMPACT,
	MAX_VALUE
};

//! 진동 스타일
enum class EVibrateStyle {
	NONE = -1,
	LIGHT,
	MEDIUM,
	HEAVY,
	MAX_VALUE
};

#endif /* KGDefine_h */
