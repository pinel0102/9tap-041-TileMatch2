//
//  CDeviceMsgSender.h
//  Unity-iPhone
//
//  Created by 이상동 on 2020/01/11.
//

#import "../../Define/KGDefine.h"

NS_ASSUME_NONNULL_BEGIN

//! 디바이스 메세지 전송자
@interface CDeviceMsgSender : NSObject {
	
}

//! 디바이스 식별자 반환 메세지를 전송한다
- (void)sendGetDeviceIDMsg:(NSString *)a_oDeviceID;

//! 국가 코드 반환 메세지를 전송한다
- (void)sendGetCountryCodeMsg:(NSString *)a_pCountryCode;

//! 스토어 버전 반환 메세지를 전송한다
- (void)sendGetStoreVersionMsg:(NSString *)a_pVersion withResult:(BOOL)a_bIsSuccess;

//! 알림 창 출력 메세지를 전송한다
- (void)sendShowAlertMsg:(BOOL)a_bIsTrue;

//! 인스턴스를 반환한다
+ (instancetype)sharedInstance;
@end			// CDeviceMsgSender

NS_ASSUME_NONNULL_END
