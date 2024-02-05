//
//  CiOSPlugin.m
//  Unity-iPhone
//
//  Created by 이상동 on 2020/02/26.
//

#import "CiOSPlugin.h"
#import "Global/Function/GFunc.h"
#import "Global/Utility/Platform/CDeviceMsgSender.h"

//! 전역 변수
static CiOSPlugin *g_pInstance = nil;

//! iOS 플러그인 - Private
@interface CiOSPlugin (Private) {
	// Nothing
}

//! 디바이스 식별자 반환 메세지를 처리한다
- (void)handleGetDeviceIDMsg:(const char *)a_pszMsg;

//! 국가 코드 반환 메세지를 처리한다
- (void)handleGetCountryCodeMsg:(const char *)a_pszMsg;

//! 스토어 버전 반환 메세지를 처리한다
- (void)handleGetStoreVersionMsg:(const char *)a_pszMsg;

//! 빌드 모드 변경 메세지를 처리한다
- (void)handleSetBuildModeMsg:(const char *)a_pszMsg;

//! 알림 창 출력 메세지를 처리한다
- (void)handleShowAlertMsg:(const char *)a_pszMsg;

//! 진동 메세지를 처리한다
- (void)handleVibrateMsg:(const char *)a_pszMsg;

//! 액티비티 인디게이터 메세지를 처리한다
- (void)handleActivityIndicatorMsg:(const char *)a_pszMsg;
@end			// CiOSPlugin (Private)

extern "C" {
	//! 유니티 메세지를 처리한다
	void HandleUnityMsg(const char *a_pszCmd, const char *a_pszMsg) {
		NSLog(@"CiOSPlugin.HandleUnityMsg: %@, %@", @(a_pszCmd), @(a_pszMsg));
		
		// 디바이스 식별자 반환 메세지 일 경우
		if(strcmp(a_pszCmd, CMD_GET_DEVICE_ID) == 0) {
			[CiOSPlugin.sharedInstance handleGetDeviceIDMsg:a_pszMsg];
		}
		// 국가 코드 반환 메세지 일 경우
		else if(strcmp(a_pszCmd, CMD_GET_COUNTRY_CODE) == 0) {
			[CiOSPlugin.sharedInstance handleGetCountryCodeMsg:a_pszMsg];
		}
		// 스토어 버전 반환 메세지 일 경우
		else if(strcmp(a_pszCmd, CMD_GET_STORE_VERSION) == 0) {
			[CiOSPlugin.sharedInstance handleGetStoreVersionMsg:a_pszMsg];
		}
		// 빌드 모드 변경 메세지 일 경우
		else if(strcmp(a_pszCmd, CMD_SET_BUILD_MODE) == 0) {
			[CiOSPlugin.sharedInstance handleSetBuildModeMsg:a_pszMsg];
		}
		// 알림 창 출력 메세지 일 경우
		else if(strcmp(a_pszCmd, CMD_SHOW_ALERT) == 0) {
			[CiOSPlugin.sharedInstance handleShowAlertMsg:a_pszMsg];
		}
		// 진동 메세지 일 경우
		else if(strcmp(a_pszCmd, CMD_VIBRATE) == 0) {
			[CiOSPlugin.sharedInstance handleVibrateMsg:a_pszMsg];
		}
		// 액티비티 인디게이터 메세지 일 경우
		else if(strcmp(a_pszCmd, CMD_ACTIVITY_INDICATOR) == 0) {
			[CiOSPlugin.sharedInstance handleActivityIndicatorMsg:a_pszMsg];
		}
	}
}

//! iOS 플러그인
@implementation CiOSPlugin
#pragma mark - Property
@synthesize buildMode;

@synthesize deviceID = m_pDeviceID;
@synthesize keychainItemWrapper = m_pKeychainItemWrapper;
@synthesize activityIndicatorView = m_pActivityIndicatorView;

@synthesize impactGeneratorList = m_pImpactGeneratorList;
@synthesize selectionGenerator = m_pSelectionGenerator;
@synthesize notificationGenerator = m_pNotificationGenerator;

#pragma mark - init
//! 객체를 생성한다
+ (id)alloc {
	@synchronized(CiOSPlugin.class) {
		// 인스턴스가 없을 경우
		if(g_pInstance == nil) {
			g_pInstance = [[super alloc] init];
			[FBAdSettings setAdvertiserTrackingEnabled:YES];
		}
	}
	
	return g_pInstance;
}

#pragma mark - instance method
//! 디바이스 식별자를 반환한다
- (NSString *)deviceID {
	// 디바이스 식별자가 유효하지 않을 경우
	if(!Func::IsValid(m_pDeviceID)) {
		m_pDeviceID = (NSString *)[self.keychainItemWrapper objectForKey:(__bridge id)kSecAttrAccount];
	}
	
	return m_pDeviceID;
}

//! 키체인 아이템 래퍼를 반환한다
- (KeychainItemWrapper *)keychainItemWrapper {
	// 키체인 아이템 래퍼가 없을 경우
	if(m_pKeychainItemWrapper == nil) {
		m_pKeychainItemWrapper = [[KeychainItemWrapper alloc] initWithIdentifier:@(ID_KEYCHAIN_DEVICE)
																	 accessGroup:nil];
	}
	
	return m_pKeychainItemWrapper;
}

//! 액티비티 인디게이터 뷰를 반환한다
- (UIActivityIndicatorView *)activityIndicatorView {
	// 액티비티 인디게이터가 없을 경우
	if(m_pActivityIndicatorView == nil) {
		UIActivityIndicatorViewStyle eIndicatorViewStyle = UIActivityIndicatorViewStyleWhiteLarge;
		
		// 새로운 액티비티 인디게이터를 지원 할 경우
		if(@available(iOS MIN_VERSION_ACTIVITY_INDICATOR, *)) {
			eIndicatorViewStyle = UIActivityIndicatorViewStyleLarge;
		}
		
		m_pActivityIndicatorView = [[UIActivityIndicatorView alloc] initWithActivityIndicatorStyle:eIndicatorViewStyle];
		m_pActivityIndicatorView.color = [UIColor colorWithWhite:1.0f alpha:1.0f];
		m_pActivityIndicatorView.center = self.rootViewController.view.center;
		m_pActivityIndicatorView.hidesWhenStopped = YES;
		
		// 크기를 설정한다 {
		float fSize = MIN(self.rootViewController.view.bounds.size.width, self.rootViewController.view.bounds.size.height);
		fSize *= SCALE_ACTIVITY_INDICATOR;
		
		float fScaleX = fSize / m_pActivityIndicatorView.bounds.size.width;
		float fScaleY = fSize / m_pActivityIndicatorView.bounds.size.height;
		
		CGAffineTransform stTransform = m_pActivityIndicatorView.transform;
		m_pActivityIndicatorView.transform = CGAffineTransformScale(stTransform, fScaleX, fScaleY);
		// 크기를 설정한다 }
		
		// 위치를 설정한다 {
		float fOffset = MIN(self.rootViewController.view.bounds.size.width, self.rootViewController.view.bounds.size.height);
		fOffset *= SCALE_ACTIVITY_INDICATOR_OFFSET;
		
		stTransform = m_pActivityIndicatorView.transform;
		m_pActivityIndicatorView.transform = CGAffineTransformTranslate(stTransform, 0.0f, -fOffset);
		// 위치를 설정한다 }
		
		[self.rootViewController.view addSubview:m_pActivityIndicatorView];
	}
	
	return m_pActivityIndicatorView;
}

//! 충격 피드백 생성자 리스트를 반환한다
- (NSArray *)impactGeneratorList {
	// 충격 피드백 생성자 리스트가 없을 경우
	if(m_pImpactGeneratorList == nil) {
		UIImpactFeedbackGenerator *pLightGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
		UIImpactFeedbackGenerator *pMediumGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
		UIImpactFeedbackGenerator *pHeavyGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
		
		m_pImpactGeneratorList = [NSArray arrayWithObjects:pLightGenerator, pMediumGenerator, pHeavyGenerator, nil];
	}
	
	return m_pImpactGeneratorList;
}

//! 선택 피드백 생성자를 반환한다
- (UISelectionFeedbackGenerator *)selectionGenerator {
	// 선택 피드백 생성자가 없을 경우
	if(m_pSelectionGenerator == nil) {
		m_pSelectionGenerator = [[UISelectionFeedbackGenerator alloc] init];
	}
	
	return m_pSelectionGenerator;
}

//! 알림 피드백 생성자를 반환한다
- (UINotificationFeedbackGenerator *)notificationGenerator {
	// 알림 피드백 생성자가 없을 경우
	if(m_pNotificationGenerator == nil) {
		m_pNotificationGenerator = [[UINotificationFeedbackGenerator alloc] init];
	}
	
	return m_pNotificationGenerator;
}

//! 루트 뷰 컨트롤러를 반환한다
- (UIViewController *)rootViewController {
	return self.unityAppController.rootViewController;
}

//! 유니티 앱 컨트롤러를 반환한다
- (UnityAppController *)unityAppController {
	return (UnityAppController *)UIApplication.sharedApplication.delegate;
}

//! 디바이스 식별자 반환 메세지를 처리한다
- (void)handleGetDeviceIDMsg:(const char *)a_pszMsg {
	NSLog(@"CiOSPlugin.handleGetDeviceIDMsg: %@", @(a_pszMsg));
	
	// 디바이스 식별자가 유효하지 않을 경우
	if(!Func::IsValid(self.deviceID)) {
		// UUID 를 지원 할 경우
		if(@available(iOS MIN_VERSION_DEVICE_ID_FOR_VENDOR, *)) {
			self.deviceID = UIDevice.currentDevice.identifierForVendor.UUIDString;
		} else {
			CFUUIDRef pUUID = CFUUIDCreate(kCFAllocatorDefault);
			self.deviceID = (__bridge NSString *)CFUUIDCreateString(kCFAllocatorDefault, pUUID);
		}
		
		[self.keychainItemWrapper setObject:self.deviceID forKey:(__bridge id)kSecAttrAccount];
	}
	
	[CDeviceMsgSender.sharedInstance sendGetDeviceIDMsg:self.deviceID];
}

//! 국가 코드 반환 메세지를 처리한다
- (void)handleGetCountryCodeMsg:(const char *)a_pszMsg {
	NSLog(@"CiOSPlugin.handleGetCountryCodeMsg: %@", @(a_pszMsg));
	
	NSLocale *pLocale = NSLocale.currentLocale;
	[CDeviceMsgSender.sharedInstance sendGetCountryCodeMsg:pLocale.countryCode];
}

//! 스토어 버전 반환 메세지를 처리한다
- (void)handleGetStoreVersionMsg:(const char *)a_pszMsg {
	NSLog(@"CiOSPlugin.handleGetStoreVersionMsg: %@", @(a_pszMsg));
	NSDictionary *pDataList = (NSDictionary *)Func::ConvertJSONStringToObj(@(a_pszMsg), NULL);
	
	NSString *pAppID = (NSString *)[pDataList objectForKey:@(KEY_APP_ID)];
	NSString *pVersion = (NSString *)[pDataList objectForKey:@(KEY_VERSION)];
	NSString *pTimeout = (NSString *)[pDataList objectForKey:@(KEY_TIMEOUT)];
	
	// 디버그 모드 일 경우
	if([self.buildMode isEqualToString:@(BUILD_MODE_DEBUG)]) {
		[CDeviceMsgSender.sharedInstance sendGetStoreVersionMsg:pVersion withResult:YES];
	} else {
		NSString *pURL = [NSString stringWithFormat:@(URL_FORMAT_STORE_VERSION), pAppID];
		NSMutableURLRequest * pURLRequest = Func::MakeURLRequest(pURL, @(HTTP_METHOD_GET), pTimeout.doubleValue);
		
		// 데이터를 수신했을 경우
		[NSURLSession.sharedSession dataTaskWithRequest:pURLRequest completionHandler:^void(NSData *a_pData, NSURLResponse *a_pResponse, NSError *a_pError) {
			NSLog(@"CiOSPlugin.onHandleGetStoreVersionMsg: %@", a_pData);
			
			// 스토어 버전 로드에 실패했을 경우
			if(a_pError != nil || (a_pData == nil || a_pResponse == nil)) {
				NSLog(@"CiOSPlugin.onHandleGetStoreVersionMsg Fail: %@", a_pError);
				[CDeviceMsgSender.sharedInstance sendGetStoreVersionMsg:pVersion withResult:NO];
			} else {
				NSString *pString = [[NSString alloc] initWithData:a_pData encoding:NSUTF8StringEncoding];
				NSDictionary *pResponseDataList = (NSDictionary *)Func::ConvertJSONStringToObj(pString, NULL);
				
				NSArray *pVersionInfoList = (NSArray *)[pResponseDataList objectForKey:@(KEY_STORE_VERSION_RESULT)];
				NSDictionary *pVersionInfo = (NSDictionary *)[pVersionInfoList lastObject];
				
				NSString *pStoreVersion = (NSString *)[pVersionInfo objectForKey:@(KEY_STORE_VERSION)];
				NSLog(@"CiOSPlugin.onHandleGetStoreVersionMsg Success: %@", pStoreVersion);
				
				// 스토어 버전이 유효 할 경우
				if(Func::IsValid(pStoreVersion)) {
					[CDeviceMsgSender.sharedInstance sendGetStoreVersionMsg:pStoreVersion withResult:YES];
				} else {
					[CDeviceMsgSender.sharedInstance sendGetStoreVersionMsg:pVersion withResult:NO];
				}
			}
		}];
	}
}

//! 빌드 모드 변경 메세지를 처리한다
- (void)handleSetBuildModeMsg:(const char *)a_pszMsg {
	NSLog(@"CiOSPlugin.handleSetBuildModeMsg: %@", @(a_pszMsg));
	self.buildMode = @(a_pszMsg);
}

//! 알림 창 출력 메세지를 처리한다
- (void)handleShowAlertMsg:(const char *)a_pszMsg {
	NSLog(@"CiOSPlugin.handleShowAlertMsg: %@", @(a_pszMsg));
	NSDictionary *pDataList = (NSDictionary *)Func::ConvertJSONStringToObj(@(a_pszMsg), NULL);
	
	NSString *pTitle = (NSString *)[pDataList objectForKey:@(KEY_ALERT_TITLE)];
	NSString *pMsg = (NSString *)[pDataList objectForKey:@(KEY_ALERT_MSG)];
	NSString *pOKBtnText = (NSString *)[pDataList objectForKey:@(KEY_ALERT_OK_BTN_TEXT)];
	NSString *pCancelBtnText = (NSString *)[pDataList objectForKey:@(KEY_ALERT_CANCEL_BTN_TEXT)];
	
	UIAlertController *pAlertController = [UIAlertController alertControllerWithTitle:Func::IsValid(pTitle) ? pTitle : nil
																			  message:pMsg
																	   preferredStyle:UIAlertControllerStyleAlert];
	
	// 확인 버튼을 눌렀을 경우
	[pAlertController addAction:[UIAlertAction actionWithTitle:pOKBtnText
														 style:UIAlertActionStyleDefault
													   handler:^void(UIAlertAction *a_pSender)
	{
		[CDeviceMsgSender.sharedInstance sendShowAlertMsg:YES];
	}]];
	
	// 취소 버튼 텍스트가 유효 할 경우
	if(Func::IsValid(pCancelBtnText)) {
		// 취소 버튼을 눌렀을 경우
		[pAlertController addAction:[UIAlertAction actionWithTitle:pCancelBtnText
															 style:UIAlertActionStyleCancel
														   handler:^void(UIAlertAction *a_pSender)
		{
			[CDeviceMsgSender.sharedInstance sendShowAlertMsg:NO];
		}]];
	}
	
	// 알림 창을 출력한다
	[self.rootViewController presentViewController:pAlertController animated:YES completion:NULL];
}

//! 진동 메세지를 처리한다
- (void)handleVibrateMsg:(const char *)a_pszMsg {
	NSLog(@"CiOSPlugin.handleVibrateMsg: %@", @(a_pszMsg));
	NSDictionary *pDataList = (NSDictionary *)Func::ConvertJSONStringToObj(@(a_pszMsg), NULL);
	
	NSString *pType = (NSString *)[pDataList objectForKey:@(KEY_VIBRATE_TYPE)];
	NSString *pStyle = (NSString *)[pDataList objectForKey:@(KEY_VIBRATE_STYLE)];
	
	EVibrateType eVibrateType = (EVibrateType)pType.intValue;
	EVibrateStyle eVibrateStyle = (EVibrateStyle)pStyle.intValue;
	
	// 진동 타입이 유효 할 경우
	if(Func::IsValid(eVibrateType)) {
		// 햅틱 진동을 지원 할 경우
		if(@available(iOS MIN_VERSION_FEEDBACK_GENERATOR, *)) {
			// 선택 진동 모드 일 경우
			if(eVibrateType == EVibrateType::SELECTION) {
				[self.selectionGenerator prepare];
				[self.selectionGenerator selectionChanged];
			}
			// 알림 진동 모드 일 경우
			else if(eVibrateType == EVibrateType::NOTIFICATION) {
				[self.notificationGenerator prepare];
				[self.notificationGenerator notificationOccurred:(UINotificationFeedbackType)eVibrateStyle];
			} else {
				UIImpactFeedbackStyle eFeedbackStyle = (UIImpactFeedbackStyle)eVibrateStyle;
				UIImpactFeedbackGenerator *pImpactGenerator = (UIImpactFeedbackGenerator *)[self.impactGeneratorList objectAtIndex:eFeedbackStyle];
				
				[pImpactGenerator prepare];
				
				// 진동 세기를 지원 할 경우
				if(@available(iOS MIN_VERSION_IMPACT_INTENSITY, *)) {
					NSString *pIntensity = (NSString *)[pDataList objectForKey:@(KEY_VIBRATE_INTENSITY)];
					[pImpactGenerator impactOccurredWithIntensity:pIntensity.floatValue];
				} else {
					[pImpactGenerator impactOccurred];
				}
			}
		} else {
			SystemSoundID nSndID = SYSTEM_SND_ID_LIGHT;
			
			// 중간 세기 일 경우
			if(eVibrateStyle == EVibrateStyle::MEDIUM) {
				nSndID = SYSTEM_SND_ID_MEDIUM;
			}
			// 최대 세기 일 경우
			else if(eVibrateStyle == EVibrateStyle::HEAVY) {
				nSndID = SYSTEM_SND_ID_HEAVY;
			}
			
			AudioServicesPlaySystemSound(nSndID);
		}
	}
}

//! 액티비티 인디게이터 메세지를 처리한다
- (void)handleActivityIndicatorMsg:(const char *)a_pszMsg {
	NSLog(@"CiOSPlugin.handleStartActivityIndicatorMsg: %@", @(a_pszMsg));
	
	// 출력 상태 일 경우
	if(Func::ConvertStringToBool(@(a_pszMsg))) {
		[self.activityIndicatorView startAnimating];
	} else {
		[self.activityIndicatorView stopAnimating];
	}
}

#pragma mark - class method
//! 인스턴스를 반환한다
+ (instancetype)sharedInstance {
	@synchronized(CiOSPlugin.class) {
		// 인스턴스가 없을 경우
		if(g_pInstance == nil) {
			g_pInstance = [[CiOSPlugin alloc] init];
		}
	}
	
	return g_pInstance;
}
@end			// CiOSPlugin
