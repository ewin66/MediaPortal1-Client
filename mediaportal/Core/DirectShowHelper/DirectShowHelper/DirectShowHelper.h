

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 6.00.0361 */
/* at Fri May 27 14:55:40 2005
 */
/* Compiler settings for .\DirectShowHelper.idl:
    Oicf, W1, Zp8, env=Win32 (32b run)
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
//@@MIDL_FILE_HEADING(  )

#pragma warning( disable: 4049 )  /* more than 64k source lines */


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 475
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif // __RPCNDR_H_VERSION__

#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __DirectShowHelper_h__
#define __DirectShowHelper_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __IVMR9Callback_FWD_DEFINED__
#define __IVMR9Callback_FWD_DEFINED__
typedef interface IVMR9Callback IVMR9Callback;
#endif 	/* __IVMR9Callback_FWD_DEFINED__ */


#ifndef __IVMR9Helper_FWD_DEFINED__
#define __IVMR9Helper_FWD_DEFINED__
typedef interface IVMR9Helper IVMR9Helper;
#endif 	/* __IVMR9Helper_FWD_DEFINED__ */


#ifndef __IDVD_FWD_DEFINED__
#define __IDVD_FWD_DEFINED__
typedef interface IDVD IDVD;
#endif 	/* __IDVD_FWD_DEFINED__ */


#ifndef __IStreamBufferRecorder_FWD_DEFINED__
#define __IStreamBufferRecorder_FWD_DEFINED__
typedef interface IStreamBufferRecorder IStreamBufferRecorder;
#endif 	/* __IStreamBufferRecorder_FWD_DEFINED__ */


#ifndef __VMR9Callback_FWD_DEFINED__
#define __VMR9Callback_FWD_DEFINED__

#ifdef __cplusplus
typedef class VMR9Callback VMR9Callback;
#else
typedef struct VMR9Callback VMR9Callback;
#endif /* __cplusplus */

#endif 	/* __VMR9Callback_FWD_DEFINED__ */


#ifndef __VMR9Helper_FWD_DEFINED__
#define __VMR9Helper_FWD_DEFINED__

#ifdef __cplusplus
typedef class VMR9Helper VMR9Helper;
#else
typedef struct VMR9Helper VMR9Helper;
#endif /* __cplusplus */

#endif 	/* __VMR9Helper_FWD_DEFINED__ */


#ifndef __DVD_FWD_DEFINED__
#define __DVD_FWD_DEFINED__

#ifdef __cplusplus
typedef class DVD DVD;
#else
typedef struct DVD DVD;
#endif /* __cplusplus */

#endif 	/* __DVD_FWD_DEFINED__ */


#ifndef __StreamBufferRecorder_FWD_DEFINED__
#define __StreamBufferRecorder_FWD_DEFINED__

#ifdef __cplusplus
typedef class StreamBufferRecorder StreamBufferRecorder;
#else
typedef struct StreamBufferRecorder StreamBufferRecorder;
#endif /* __cplusplus */

#endif 	/* __StreamBufferRecorder_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"
#include "amstream.h"

#ifdef __cplusplus
extern "C"{
#endif 

void * __RPC_USER MIDL_user_allocate(size_t);
void __RPC_USER MIDL_user_free( void * ); 

/* interface __MIDL_itf_DirectShowHelper_0000 */
/* [local] */ 

#if 0
typedef DWORD IDirect3DTexture9;

#endif


extern RPC_IF_HANDLE __MIDL_itf_DirectShowHelper_0000_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_DirectShowHelper_0000_v0_0_s_ifspec;

#ifndef __IVMR9Callback_INTERFACE_DEFINED__
#define __IVMR9Callback_INTERFACE_DEFINED__

/* interface IVMR9Callback */
/* [unique][helpstring][uuid][object] */ 


EXTERN_C const IID IID_IVMR9Callback;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("6BD43BC8-22A3-478C-A571-EA723BD9F019")
    IVMR9Callback : public IUnknown
    {
    public:
        virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE PresentImage( 
            /* [in] */ int width,
            /* [in] */ int height,
            /* [in] */ int ARWidth,
            /* [in] */ int ARHeight,
            /* [in] */ DWORD texture) = 0;
        
        virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE PresentSurface( 
            /* [in] */ int width,
            /* [in] */ int height,
            /* [in] */ int ARWidth,
            /* [in] */ int ARHeight,
            /* [in] */ DWORD surface) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IVMR9CallbackVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IVMR9Callback * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IVMR9Callback * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IVMR9Callback * This);
        
        /* [helpstring] */ HRESULT ( STDMETHODCALLTYPE *PresentImage )( 
            IVMR9Callback * This,
            /* [in] */ int width,
            /* [in] */ int height,
            /* [in] */ int ARWidth,
            /* [in] */ int ARHeight,
            /* [in] */ DWORD texture);
        
        /* [helpstring] */ HRESULT ( STDMETHODCALLTYPE *PresentSurface )( 
            IVMR9Callback * This,
            /* [in] */ int width,
            /* [in] */ int height,
            /* [in] */ int ARWidth,
            /* [in] */ int ARHeight,
            /* [in] */ DWORD surface);
        
        END_INTERFACE
    } IVMR9CallbackVtbl;

    interface IVMR9Callback
    {
        CONST_VTBL struct IVMR9CallbackVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IVMR9Callback_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IVMR9Callback_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IVMR9Callback_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define IVMR9Callback_PresentImage(This,width,height,ARWidth,ARHeight,texture)	\
    (This)->lpVtbl -> PresentImage(This,width,height,ARWidth,ARHeight,texture)

#define IVMR9Callback_PresentSurface(This,width,height,ARWidth,ARHeight,surface)	\
    (This)->lpVtbl -> PresentSurface(This,width,height,ARWidth,ARHeight,surface)

#endif /* COBJMACROS */


#endif 	/* C style interface */



/* [helpstring] */ HRESULT STDMETHODCALLTYPE IVMR9Callback_PresentImage_Proxy( 
    IVMR9Callback * This,
    /* [in] */ int width,
    /* [in] */ int height,
    /* [in] */ int ARWidth,
    /* [in] */ int ARHeight,
    /* [in] */ DWORD texture);


void __RPC_STUB IVMR9Callback_PresentImage_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring] */ HRESULT STDMETHODCALLTYPE IVMR9Callback_PresentSurface_Proxy( 
    IVMR9Callback * This,
    /* [in] */ int width,
    /* [in] */ int height,
    /* [in] */ int ARWidth,
    /* [in] */ int ARHeight,
    /* [in] */ DWORD surface);


void __RPC_STUB IVMR9Callback_PresentSurface_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __IVMR9Callback_INTERFACE_DEFINED__ */


#ifndef __IVMR9Helper_INTERFACE_DEFINED__
#define __IVMR9Helper_INTERFACE_DEFINED__

/* interface IVMR9Helper */
/* [unique][helpstring][uuid][object] */ 


EXTERN_C const IID IID_IVMR9Helper;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("1463CD20-B360-49B8-A81C-47981347FDB5")
    IVMR9Helper : public IUnknown
    {
    public:
        virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE Init( 
            /* [in] */ IVMR9Callback *callback,
            /* [in] */ DWORD dwD3DDevice,
            /* [in] */ IBaseFilter *vmr9Filter,
            /* [in] */ DWORD monitor) = 0;
        
        virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetDeinterlacePrefs( 
            /* [in] */ DWORD dwInterlace) = 0;
        
        virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetDeinterlaceMode( void) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IVMR9HelperVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IVMR9Helper * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IVMR9Helper * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IVMR9Helper * This);
        
        /* [helpstring] */ HRESULT ( STDMETHODCALLTYPE *Init )( 
            IVMR9Helper * This,
            /* [in] */ IVMR9Callback *callback,
            /* [in] */ DWORD dwD3DDevice,
            /* [in] */ IBaseFilter *vmr9Filter,
            /* [in] */ DWORD monitor);
        
        /* [helpstring] */ HRESULT ( STDMETHODCALLTYPE *SetDeinterlacePrefs )( 
            IVMR9Helper * This,
            /* [in] */ DWORD dwInterlace);
        
        /* [helpstring] */ HRESULT ( STDMETHODCALLTYPE *SetDeinterlaceMode )( 
            IVMR9Helper * This);
        
        END_INTERFACE
    } IVMR9HelperVtbl;

    interface IVMR9Helper
    {
        CONST_VTBL struct IVMR9HelperVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IVMR9Helper_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IVMR9Helper_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IVMR9Helper_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define IVMR9Helper_Init(This,callback,dwD3DDevice,vmr9Filter,monitor)	\
    (This)->lpVtbl -> Init(This,callback,dwD3DDevice,vmr9Filter,monitor)

#define IVMR9Helper_SetDeinterlacePrefs(This,dwInterlace)	\
    (This)->lpVtbl -> SetDeinterlacePrefs(This,dwInterlace)

#define IVMR9Helper_SetDeinterlaceMode(This)	\
    (This)->lpVtbl -> SetDeinterlaceMode(This)

#endif /* COBJMACROS */


#endif 	/* C style interface */



/* [helpstring] */ HRESULT STDMETHODCALLTYPE IVMR9Helper_Init_Proxy( 
    IVMR9Helper * This,
    /* [in] */ IVMR9Callback *callback,
    /* [in] */ DWORD dwD3DDevice,
    /* [in] */ IBaseFilter *vmr9Filter,
    /* [in] */ DWORD monitor);


void __RPC_STUB IVMR9Helper_Init_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring] */ HRESULT STDMETHODCALLTYPE IVMR9Helper_SetDeinterlacePrefs_Proxy( 
    IVMR9Helper * This,
    /* [in] */ DWORD dwInterlace);


void __RPC_STUB IVMR9Helper_SetDeinterlacePrefs_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring] */ HRESULT STDMETHODCALLTYPE IVMR9Helper_SetDeinterlaceMode_Proxy( 
    IVMR9Helper * This);


void __RPC_STUB IVMR9Helper_SetDeinterlaceMode_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __IVMR9Helper_INTERFACE_DEFINED__ */


#ifndef __IDVD_INTERFACE_DEFINED__
#define __IDVD_INTERFACE_DEFINED__

/* interface IDVD */
/* [unique][helpstring][nonextensible][dual][uuid][object] */ 


EXTERN_C const IID IID_IDVD;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("C5B0E1CE-6BFE-48FC-8EC0-8ABC19C1EBD4")
    IDVD : public IDispatch
    {
    public:
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE Reset( 
            /* [in] */ BSTR strPath) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IDVDVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IDVD * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IDVD * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IDVD * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfoCount )( 
            IDVD * This,
            /* [out] */ UINT *pctinfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfo )( 
            IDVD * This,
            /* [in] */ UINT iTInfo,
            /* [in] */ LCID lcid,
            /* [out] */ ITypeInfo **ppTInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetIDsOfNames )( 
            IDVD * This,
            /* [in] */ REFIID riid,
            /* [size_is][in] */ LPOLESTR *rgszNames,
            /* [in] */ UINT cNames,
            /* [in] */ LCID lcid,
            /* [size_is][out] */ DISPID *rgDispId);
        
        /* [local] */ HRESULT ( STDMETHODCALLTYPE *Invoke )( 
            IDVD * This,
            /* [in] */ DISPID dispIdMember,
            /* [in] */ REFIID riid,
            /* [in] */ LCID lcid,
            /* [in] */ WORD wFlags,
            /* [out][in] */ DISPPARAMS *pDispParams,
            /* [out] */ VARIANT *pVarResult,
            /* [out] */ EXCEPINFO *pExcepInfo,
            /* [out] */ UINT *puArgErr);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *Reset )( 
            IDVD * This,
            /* [in] */ BSTR strPath);
        
        END_INTERFACE
    } IDVDVtbl;

    interface IDVD
    {
        CONST_VTBL struct IDVDVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IDVD_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IDVD_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IDVD_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define IDVD_GetTypeInfoCount(This,pctinfo)	\
    (This)->lpVtbl -> GetTypeInfoCount(This,pctinfo)

#define IDVD_GetTypeInfo(This,iTInfo,lcid,ppTInfo)	\
    (This)->lpVtbl -> GetTypeInfo(This,iTInfo,lcid,ppTInfo)

#define IDVD_GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)	\
    (This)->lpVtbl -> GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)

#define IDVD_Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)	\
    (This)->lpVtbl -> Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)


#define IDVD_Reset(This,strPath)	\
    (This)->lpVtbl -> Reset(This,strPath)

#endif /* COBJMACROS */


#endif 	/* C style interface */



/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IDVD_Reset_Proxy( 
    IDVD * This,
    /* [in] */ BSTR strPath);


void __RPC_STUB IDVD_Reset_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __IDVD_INTERFACE_DEFINED__ */


#ifndef __IStreamBufferRecorder_INTERFACE_DEFINED__
#define __IStreamBufferRecorder_INTERFACE_DEFINED__

/* interface IStreamBufferRecorder */
/* [unique][helpstring][nonextensible][dual][uuid][object] */ 


EXTERN_C const IID IID_IStreamBufferRecorder;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("752D4561-E632-4328-8206-9FB5277D8096")
    IStreamBufferRecorder : public IDispatch
    {
    public:
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE Create( 
            IBaseFilter *streamBufferSink,
            /* [in] */ BSTR strPath,
            DWORD dwRecordingType) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE Start( 
            LONG startTime) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE Stop( void) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE SetAttributeString( 
            /* [in] */ BSTR strName,
            /* [in] */ BSTR strValue) = 0;
        
        virtual /* [helpstring][id] */ HRESULT STDMETHODCALLTYPE SetAttributeDWORD( 
            /* [in] */ BSTR strName,
            ULONG dwValue) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IStreamBufferRecorderVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IStreamBufferRecorder * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IStreamBufferRecorder * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IStreamBufferRecorder * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfoCount )( 
            IStreamBufferRecorder * This,
            /* [out] */ UINT *pctinfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfo )( 
            IStreamBufferRecorder * This,
            /* [in] */ UINT iTInfo,
            /* [in] */ LCID lcid,
            /* [out] */ ITypeInfo **ppTInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetIDsOfNames )( 
            IStreamBufferRecorder * This,
            /* [in] */ REFIID riid,
            /* [size_is][in] */ LPOLESTR *rgszNames,
            /* [in] */ UINT cNames,
            /* [in] */ LCID lcid,
            /* [size_is][out] */ DISPID *rgDispId);
        
        /* [local] */ HRESULT ( STDMETHODCALLTYPE *Invoke )( 
            IStreamBufferRecorder * This,
            /* [in] */ DISPID dispIdMember,
            /* [in] */ REFIID riid,
            /* [in] */ LCID lcid,
            /* [in] */ WORD wFlags,
            /* [out][in] */ DISPPARAMS *pDispParams,
            /* [out] */ VARIANT *pVarResult,
            /* [out] */ EXCEPINFO *pExcepInfo,
            /* [out] */ UINT *puArgErr);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *Create )( 
            IStreamBufferRecorder * This,
            IBaseFilter *streamBufferSink,
            /* [in] */ BSTR strPath,
            DWORD dwRecordingType);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *Start )( 
            IStreamBufferRecorder * This,
            LONG startTime);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *Stop )( 
            IStreamBufferRecorder * This);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *SetAttributeString )( 
            IStreamBufferRecorder * This,
            /* [in] */ BSTR strName,
            /* [in] */ BSTR strValue);
        
        /* [helpstring][id] */ HRESULT ( STDMETHODCALLTYPE *SetAttributeDWORD )( 
            IStreamBufferRecorder * This,
            /* [in] */ BSTR strName,
            ULONG dwValue);
        
        END_INTERFACE
    } IStreamBufferRecorderVtbl;

    interface IStreamBufferRecorder
    {
        CONST_VTBL struct IStreamBufferRecorderVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IStreamBufferRecorder_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IStreamBufferRecorder_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IStreamBufferRecorder_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define IStreamBufferRecorder_GetTypeInfoCount(This,pctinfo)	\
    (This)->lpVtbl -> GetTypeInfoCount(This,pctinfo)

#define IStreamBufferRecorder_GetTypeInfo(This,iTInfo,lcid,ppTInfo)	\
    (This)->lpVtbl -> GetTypeInfo(This,iTInfo,lcid,ppTInfo)

#define IStreamBufferRecorder_GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)	\
    (This)->lpVtbl -> GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)

#define IStreamBufferRecorder_Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)	\
    (This)->lpVtbl -> Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)


#define IStreamBufferRecorder_Create(This,streamBufferSink,strPath,dwRecordingType)	\
    (This)->lpVtbl -> Create(This,streamBufferSink,strPath,dwRecordingType)

#define IStreamBufferRecorder_Start(This,startTime)	\
    (This)->lpVtbl -> Start(This,startTime)

#define IStreamBufferRecorder_Stop(This)	\
    (This)->lpVtbl -> Stop(This)

#define IStreamBufferRecorder_SetAttributeString(This,strName,strValue)	\
    (This)->lpVtbl -> SetAttributeString(This,strName,strValue)

#define IStreamBufferRecorder_SetAttributeDWORD(This,strName,dwValue)	\
    (This)->lpVtbl -> SetAttributeDWORD(This,strName,dwValue)

#endif /* COBJMACROS */


#endif 	/* C style interface */



/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IStreamBufferRecorder_Create_Proxy( 
    IStreamBufferRecorder * This,
    IBaseFilter *streamBufferSink,
    /* [in] */ BSTR strPath,
    DWORD dwRecordingType);


void __RPC_STUB IStreamBufferRecorder_Create_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IStreamBufferRecorder_Start_Proxy( 
    IStreamBufferRecorder * This,
    LONG startTime);


void __RPC_STUB IStreamBufferRecorder_Start_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IStreamBufferRecorder_Stop_Proxy( 
    IStreamBufferRecorder * This);


void __RPC_STUB IStreamBufferRecorder_Stop_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IStreamBufferRecorder_SetAttributeString_Proxy( 
    IStreamBufferRecorder * This,
    /* [in] */ BSTR strName,
    /* [in] */ BSTR strValue);


void __RPC_STUB IStreamBufferRecorder_SetAttributeString_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring][id] */ HRESULT STDMETHODCALLTYPE IStreamBufferRecorder_SetAttributeDWORD_Proxy( 
    IStreamBufferRecorder * This,
    /* [in] */ BSTR strName,
    ULONG dwValue);


void __RPC_STUB IStreamBufferRecorder_SetAttributeDWORD_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __IStreamBufferRecorder_INTERFACE_DEFINED__ */



#ifndef __DirectShowHelperLib_LIBRARY_DEFINED__
#define __DirectShowHelperLib_LIBRARY_DEFINED__

/* library DirectShowHelperLib */
/* [helpstring][version][uuid] */ 


EXTERN_C const IID LIBID_DirectShowHelperLib;

EXTERN_C const CLSID CLSID_VMR9Callback;

#ifdef __cplusplus

class DECLSPEC_UUID("A7D8DDD4-2104-42C0-966C-08B400F5498F")
VMR9Callback;
#endif

EXTERN_C const CLSID CLSID_VMR9Helper;

#ifdef __cplusplus

class DECLSPEC_UUID("D23CF2BC-5AD3-407F-B562-A7CB0FD342DB")
VMR9Helper;
#endif

EXTERN_C const CLSID CLSID_DVD;

#ifdef __cplusplus

class DECLSPEC_UUID("EA2F675F-E8A2-4C75-9B04-8A170B2AAC47")
DVD;
#endif

EXTERN_C const CLSID CLSID_StreamBufferRecorder;

#ifdef __cplusplus

class DECLSPEC_UUID("BBB2551F-E239-41CE-805A-651964BE143E")
StreamBufferRecorder;
#endif
#endif /* __DirectShowHelperLib_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

unsigned long             __RPC_USER  BSTR_UserSize(     unsigned long *, unsigned long            , BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserMarshal(  unsigned long *, unsigned char *, BSTR * ); 
unsigned char * __RPC_USER  BSTR_UserUnmarshal(unsigned long *, unsigned char *, BSTR * ); 
void                      __RPC_USER  BSTR_UserFree(     unsigned long *, BSTR * ); 

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


