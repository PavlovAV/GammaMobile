#ifndef DECODE_API_H
#define DECODE_API_H

/******************************************************************************/
/* Decoding API Definitions                                                   */
/******************************************************************************/

#ifdef __cplusplus
extern "C" {
#endif

#include <winerror.h>
#include "commonApi.h"
#include "decodeParam.h"

/******************************************************************************/
/* Decoding driver error codes returned via GetLastError.                     */
/******************************************************************************/

#define FACILITY_DCD			0xA0
#define MAKE_DCD_ERROR(code)	(MAKE_HRESULT(1, FACILITY_DCD, (code))|(1UL<<29))

// Parameter index is not supported.
#define DCD_E_PARAM_INDEX		MAKE_DCD_ERROR(0x0000)

// Parameter value is too low.
#define DCD_E_PARAM_MIN			MAKE_DCD_ERROR(0x0001)

// Parameter value is too high.
#define DCD_E_PARAM_MAX			MAKE_DCD_ERROR(0x0002)

// Triggering is already active.
#define DCD_E_TRIG_BUSY			MAKE_DCD_ERROR(0x0003)

// More than 255 monitor requests exist.
#define DCD_E_TOO_MANY_MONITORS	MAKE_DCD_ERROR(0x0004)

/******************************************************************************/
/* Structure Definitions                                                      */
/******************************************************************************/

#pragma pack(push)
#pragma pack(4)

//
// Used to communicate a list of parameters to the DecodeGet/DecodeSetParams() functions.
//
typedef struct
{
	DWORD index;		// Index of parameter to set (or get)
	DWORD value;		// New (or current) value of parameter
} DCD_PARAM;

//
// Used to obtain parameter limits.
//
typedef struct
{
	DWORD index;		// Index of parameter to set (or get)
	DWORD min;			// Minimum value of parameter
	DWORD max;			// Maximum value of parameter
} DCD_PARAM_LIMIT;


#pragma pack(pop)

/******************************************************************************/
/* Function Prototypes                                                        */
/******************************************************************************/

DWORD cdecl DecodeGetParam(DWORD index);

BOOL cdecl DecodeGetParams(DCD_PARAM *paramArray, int paramCount);

BOOL cdecl DecodeSetParam(DWORD index, DWORD value);

BOOL cdecl DecodeSetParams(DCD_PARAM *paramArray, int paramCount);

long cdecl DecodeTestParams(DCD_PARAM *paramArray, int paramCount);

BOOL cdecl DecodeGetParamLimits(DCD_PARAM_LIMIT *paramArray, int paramCount);

BOOL cdecl DecodeGetWedge(DWORD inputType);

BOOL cdecl DecodeSetWedge(DWORD inputType, BOOL enable);

BOOL cdecl DecodeEnumDevices(DWORD *deviceCaps);

DWORD cdecl DecodeGetDeviceCaps(HANDLE hFile);

BOOL cdecl DecodeGetFriendlyName(HANDLE hFile, DWORD *stringLen,
								 LPWSTR lpString, DWORD nMaxLen);

BOOL cdecl DecodeSoftTrigger(HANDLE hFile, DWORD inputType, DWORD timeout);

BOOL cdecl DecodeSoftTriggerStop(void);

DWORD cdecl DecodePostRequestWait(HANDLE hFile, DWORD reqType);

DWORD cdecl DecodePostRequestEvent(HANDLE hFile, DWORD reqType, HANDLE hEvent);

DWORD cdecl DecodePostRequestEventEx(HANDLE hFile, DWORD reqType,
									 HANDLE hEventRead, HANDLE hEventTimeout);

DWORD cdecl DecodePostRequestMsg(HANDLE hFile, DWORD reqType, HWND hWnd, UINT uMsg);

DWORD cdecl DecodePostRequestMsgEx(HANDLE hFile, DWORD reqType, HWND hWnd,
								   UINT uMsgRead, UINT uMsgTimeout);

BOOL cdecl DecodeCancelRequest(HANDLE hFile, DWORD reqID);

CODE_ID cdecl DecodeGetCodeID(HANDLE hFile, DWORD reqID);

BOOL cdecl DecodeReadString(HANDLE hFile, DWORD reqID, DWORD *stringLen,
							LPWSTR lpString, DWORD nMaxLen, CODE_ID *codeID);

BOOL cdecl DecodeFlushData(HANDLE hFile);

BOOL cdecl DecodeGoodRead(DWORD beepType);

/******************************************************************************/
/* Decoding Constants                                                         */
/******************************************************************************/

//
// Parameter Limit Bits
//
#define DCD_PARAM_UNKNOWN		0x80000000
#define DCD_PARAM_MIN			0x40000000
#define DCD_PARAM_MAX			0x20000000

//
// Wedge Types (in addition to Input Data Types)
//
#define DCD_CLIPBOARD			0x00010000

//
// Device Capabilities
//
#define DCD_CAP_EXISTS			0x00000001
#define DCD_CAP_BARCODE			0x00000002
#define DCD_CAP_MSR				0x00000004
#define DCD_CAP_RFID			0x00000008
#define DCD_CAP_SPOTBEAM		0x00000020

//
// Request Types
//
#define DCD_POST_COUNT_MASK		0x000000FF
#define DCD_POST_COUNT(n)		((n)&DCD_POST_COUNT_MASK)
#define DCD_POST_RECURRING		0x00000100

//
// Good Read Beep Types
//
#define DCD_BEEP_LABEL_GOOD_READ	0x00
#define DCD_BEEP_TAG_READ			0x01
#define DCD_BEEP_TAG_GOOD_WRITE		0x02
#define DCD_BEEP_TAG_BAD_WRITE		0x03

#ifdef __cplusplus
}
#endif

#endif // DECODE_API_H
