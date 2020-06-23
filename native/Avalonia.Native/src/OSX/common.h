#ifndef common_h
#define common_h
#include "comimpl.h"
#include "avalonia-native.h"
#include <stdio.h>
#import <Foundation/Foundation.h>
#import <AppKit/AppKit.h>
#include <pthread.h>

extern IAvnPlatformThreadingInterface* CreatePlatformThreading();
extern void FreeAvnGCHandle(void* handle);
extern IAvnWindow* CreateAvnWindow(IAvnWindowEvents*events, IAvnGlContext* gl);
extern IAvnPopup* CreateAvnPopup(IAvnWindowEvents*events, IAvnGlContext* gl);
extern IAvnSystemDialogs* CreateSystemDialogs();
extern IAvnScreens* CreateScreens();
extern IAvnClipboard* CreateClipboard(NSPasteboard*, NSPasteboardItem*);
extern NSPasteboardItem* TryGetPasteboardItem(IAvnClipboard*);
extern NSObject<NSDraggingSource>* CreateDraggingSource(NSDragOperation op, IAvnDndResultCallback* cb, void* handle);
extern void* GetAvnDataObjectHandleFromDraggingInfo(NSObject<NSDraggingInfo>* info);
extern NSString* GetAvnCustomDataType();
extern AvnDragDropEffects ConvertDragDropEffects(NSDragOperation nsop);
extern IAvnCursorFactory* CreateCursorFactory();
extern IAvnGlDisplay* GetGlDisplay();
extern IAvnMenu* CreateAppMenu(IAvnMenuEvents* events);
extern IAvnMenuItem* CreateAppMenuItem();
extern IAvnMenuItem* CreateAppMenuItemSeperator();
extern IAvnNativeControlHost* CreateNativeControlHost(NSView* parent);
extern void SetAppMenu (NSString* appName, IAvnMenu* appMenu);
extern IAvnMenu* GetAppMenu ();
extern NSMenuItem* GetAppMenuItem ();

extern void InitializeAvnApp();
extern NSApplicationActivationPolicy AvnDesiredActivationPolicy;
extern NSPoint ToNSPoint (AvnPoint p);
extern AvnPoint ToAvnPoint (NSPoint p);
extern AvnPoint ConvertPointY (AvnPoint p);
extern NSSize ToNSSize (AvnSize s);
#ifdef DEBUG
#define NSDebugLog(...) NSLog(__VA_ARGS__)
#else
#define NSDebugLog(...) (void)0
#endif

template<typename T> inline T* objc_cast(id from) {
    if(from == nil)
        return nil;
    if ([from isKindOfClass:[T class]]) {
        return static_cast<T*>(from);
    }
    return nil;
}

@interface ActionCallback : NSObject
- (ActionCallback*) initWithCallback: (IAvnActionCallback*) callback;
- (void) action;
@end

class AvnInsidePotentialDeadlock
{
public:
    static bool IsInside();
    AvnInsidePotentialDeadlock();
    ~AvnInsidePotentialDeadlock();
};

#endif
