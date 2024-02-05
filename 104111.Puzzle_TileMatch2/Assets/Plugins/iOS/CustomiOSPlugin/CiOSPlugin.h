//
//  CiOSPlugin.h
//  Unity-iPhone
//
//  Created by 이상동 on 2020/02/26.
//
#pragma once

#import "Global/Define/KGDefine.h"
#import "Global/Utility/External/Keychain/KeychainItemWrapper.h"
#import "UnityAppController.h"

NS_ASSUME_NONNULL_BEGIN

//! iOS 플러그인
@interface CiOSPlugin : NSObject {
	NSString *m_pDeviceID;
	KeychainItemWrapper *m_pKeychainItemWrapper;
	UIActivityIndicatorView *m_pActivityIndicatorView;
	
	NSArray *m_pImpactGeneratorList;
	UISelectionFeedbackGenerator *m_pSelectionGenerator;
	UINotificationFeedbackGenerator *m_pNotificationGenerator;
}

// 프로퍼티 {
@property (nonatomic, copy) NSString *deviceID;
@property (nonatomic, copy) NSString *buildMode;
@property (nonatomic, strong, readonly) KeychainItemWrapper *keychainItemWrapper;
@property (nonatomic, strong, readonly) UIActivityIndicatorView *activityIndicatorView;

@property (nonatomic, strong) NSArray *impactGeneratorList;
@property (nonatomic, strong) UISelectionFeedbackGenerator *selectionGenerator;
@property (nonatomic, strong) UINotificationFeedbackGenerator *notificationGenerator;

@property (nonatomic, strong, readonly) UIViewController *rootViewController;
@property (nonatomic, strong, readonly) UnityAppController *unityAppController;
// 프로퍼티 }

//! 인스턴스를 반환한다
+ (instancetype)sharedInstance;
@end			// CiOSPlugin

NS_ASSUME_NONNULL_END
