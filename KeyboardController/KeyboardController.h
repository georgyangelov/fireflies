#pragma once

#include <initguid.h>
#include <windows.h>
#include <vector>

#include "../aurora-sdk/Include/ICTLEDMgr.h"
#include "../aurora-sdk/Include/CLSID_CCTLEDMgr.h"
#include "../aurora-sdk/Include/CTIntrfc_DynamicLoad.h"
#include "../aurora-sdk/Include/CTLEDCommon.h"

#include "LEDGroupings.h"

struct ConnectedDevice {
	USHORT vendorId;
	USHORT productId;

	WCHAR serialNumber[1000];
	DWORD serialNumberSize = 1000;

	WCHAR instance[1000];
	DWORD instanceSize = 1000;

	USHORT ledInfoFlag;
	USHORT totalNumLeds;

	WCHAR friendlyName[1000];
	DWORD friendlyNameSize = 1000;
};

inline int timerCallback(CTLEDMGRTIMERPROCDATA*, LPARAM);

struct KeyboardController {
private:
	ICTLEDMgr* api;

	int groupCount = ALL_LEDS_COUNT;
	int ledsInGroup = 1;

	DWORD grouping[ALL_LEDS_COUNT][2] = {0};
	CTLEDMGRCMDPARAM_SetLedSettings ledSettings = { };

	CTLED_Pattern pattern[ALL_LEDS_COUNT] = { };
	// TODO: Remove the padding
	CTColour pixels[ALL_LEDS_COUNT + 1000] = {0};

	bool running = false;
	int targetIntervalMillis = 1;

	friend int timerCallback(CTLEDMGRTIMERPROCDATA*, LPARAM);

public:
	KeyboardController(ICTLEDMgr* api) {
		this->api = api;

		configureSingleLedGrouping(grouping);

		for (int i = 0; i < groupCount; i++) {
			pattern[i] = CTLED_Pattern_Static;
		}
	}

	~KeyboardController() {
		api->Shutdown(0);
		api->Release();
	}

	std::vector<ConnectedDevice> enumerateConnectedDevices() {
		std::vector<ConnectedDevice> devices;

		for (int i = 0; ; i++) {
			ConnectedDevice device;

			HRESULT result = api->EnumConnectedDevices(
				i,
				&device.vendorId,
				&device.productId,
				(LPWSTR)&device.serialNumber,
				&device.serialNumberSize,
				(LPWSTR)&device.instance,
				&device.instanceSize,
				&device.ledInfoFlag,
				&device.totalNumLeds,
				(LPWSTR)&device.friendlyName,
				&device.friendlyNameSize,
				0
			);

			if (!SUCCEEDED(result)) {
				break;
			}

			devices.push_back(device);
		}

		return devices;
	}

	void openDevice(ConnectedDevice* device) {
		DWORD detailErrorCode;

		HRESULT result = api->Open(
			device->vendorId,
			device->productId,
			device->serialNumber,
			device->instance,
			0,
			0,
			false,
			&detailErrorCode,
			0
		);

		if (!SUCCEEDED(result)) {
			// TODO: Error handling
			throw std::exception("Cannot open device");
		}

		configureLeds();
	}

	void closeDevice() {
		api->Close(0);
	}

	int getPixelCount() {
		return ALL_LEDS_COUNT;
	}

	CTColour* getPixelBuffer() {
		return pixels;
	}

	void fillPixelBuffer(CTColour* colors, int colorCount) {
		if (colorCount > ALL_LEDS_COUNT) {
			colorCount = ALL_LEDS_COUNT;
		}

		memcpy(pixels, colors, colorCount * sizeof(CTColour));
	}

	void sendPixels() {
		CTLEDINFOCMDPARAM_FillupAll fill = { };
		fill.pLedInfo = &ledSettings.colourLed;
		fill.dwNumLedGroups = groupCount;
		fill.pPatternIndividualLedGroupArray1D = &pattern[0];
		fill.pdwLedGroupingArray2D = &grouping[0][0];
		fill.dwNumColumnsOfLedGroupArray2D = ledsInGroup + 1;
		fill.dwNumColourLayers = 1;
		fill.pColourIndividualLedGroupArray2D = &pixels[0];
		fill.dwNumColumnsOfColourIndividualLedGroupArray2D = 1;

		HRESULT result = api->PrepareLedInfo(
			CTLEDINFOCMD_FillupAll,
			(LPARAM)&fill,
			0
		);

		if (!SUCCEEDED(result)) {
			// TODO: Error handling
			throw std::exception("Cannot prepare LED info");
		}

		CTInit_CTLEDMGRCMDPARAM_SetLedSettings(&ledSettings);
		ledSettings.fIgnoreColour = false;
		ledSettings.fIgnorePattern = false;
		ledSettings.patternLed = CTLED_Pattern_Static;

		ledSettings.fIgnorePatternDirection = true;
		ledSettings.fIgnorePatternDirectionFlag = true;
		ledSettings.flagLedPatternDirection = CTLED_PatternDirectionFlag_Looping;
		ledSettings.fIgnorePeriodicTime = true;
		ledSettings.patternLedThePeriodicTimeIsFor = CTLED_Pattern_Static;
		ledSettings.dwPeriodicTimeInMilliseconds = 0;
		ledSettings.dwPeriodicTimeInCyclesPerMinute = 0;

		result = api->ExecuteCommand(CTLEDMGRCMD_SetLedSettings, (LPARAM)&ledSettings, 0);
		if (!SUCCEEDED(result)) {
			// TODO: Error handling
			throw std::exception("Cannot set LEDs");
		}
	}

	void enableTimerSending() {
		CTTIMERINFOPARAM timerParameters = { };
		timerParameters.dwDueTimeInMilliseconds = 0;
		timerParameters.dwPeriodicTimeInMilliseconds = 1;
		//timerParameters.dwPeriodicTimeInMilliseconds = 1000 / 60;

		HRESULT result = api->RegisterTimerCallback(timerCallback, &timerParameters, (LPARAM)this, 0);

		if (!SUCCEEDED(result)) {
			throw std::exception("Cannot register timer callback");
		}

		running = true;
		targetIntervalMillis = timerParameters.dwPeriodicTimeInMilliseconds;
	}

	void disableTimerSending() {
		HRESULT result = api->UnregisterTimerCallback(0);

		if (!SUCCEEDED(result)) {
			throw std::exception("Cannot unregister timer callback");
		}

		running = false;
	}

private:

	void configureLeds() {
		CTLEDINFOCMDPARAM_Initialize initializeConfig = { };
		initializeConfig.dwMaxNumLedGroups = groupCount;
		initializeConfig.dwMaxNumLedsInEachGroup = ledsInGroup;
		initializeConfig.dwMaxNumColourLayersInEachGroup = 1;
		initializeConfig.pLedInfo = &ledSettings.colourLed;

		HRESULT result = api->PrepareLedInfo(CTLEDINFOCMD_Initialize, (LPARAM)&initializeConfig, 0);
		if (!SUCCEEDED(result)) {
			// TODO: Error handling
			throw std::exception("Cannot initialize LED config");
		}
	}
};

inline int timerCallback(CTLEDMGRTIMERPROCDATA* timerData, LPARAM userData) {
	auto controller = (KeyboardController*)userData;

	if (controller->running){
		controller->sendPixels();
	}

	return 0;
}

#define EXPORTED extern "C" __declspec(dllexport)

EXPORTED KeyboardController* create_controller() {
	HMODULE interfaceUtilsDll = LoadLibraryW(L"CTIntrfu.dll");
	ICTLEDMgr* manager;

	CTINTRFCRESULT result = CTCreateInstanceEx_Dyn(
		interfaceUtilsDll, CLSID_CCTLEDMgr, NULL, 0, IID_ICTLEDMgr, NULL, NULL,
		L"CTLEDMgr.dll", NULL, (void**)&manager
	);

	if (result != CTINTRFCRESULT_Success) {
		return nullptr;
	}

	DWORD dwFlag = 0;
	HRESULT initResult = manager->Initialize(DEFINITION_CTLEDMgr_Interface_Version, dwFlag);

	if (!SUCCEEDED(initResult)) {
		manager->Release();
		CTFreeUnusedLibrariesEx_Dyn(interfaceUtilsDll);
		return nullptr;
	}

	return new KeyboardController(manager);
}

EXPORTED size_t controller_get_connected_devices(KeyboardController* self, ConnectedDevice* connectedDevices, int connectedDevicesCapacity) {
	auto devices = self->enumerateConnectedDevices();

	if (devices.size() > connectedDevicesCapacity) {
		return 0;
	}

	for (int i = 0; i < devices.size(); i++) {
		connectedDevices[i] = devices[i];
	}

	return devices.size();
}

EXPORTED void controller_open_device(KeyboardController* self, ConnectedDevice* device) {
	self->openDevice(device);
}

EXPORTED void controller_close_device(KeyboardController* self) {
	self->closeDevice();
}

EXPORTED int controller_get_pixel_count(KeyboardController* self) {
	return self->getPixelCount();
}

EXPORTED void controller_fill_pixel_buffer(KeyboardController* self, CTColour* pixels, int pixelCount) {
	self->fillPixelBuffer(pixels, pixelCount);
}

EXPORTED CTColour* controller_get_pixel_buffer(KeyboardController* self) {
	return self->getPixelBuffer();
}

EXPORTED void controller_send_pixels(KeyboardController* self) {
	self->sendPixels();
}

EXPORTED void controller_enable_timer_sending(KeyboardController* self) {
	self->enableTimerSending();
}

EXPORTED void controller_disable_timer_sending(KeyboardController* self) {
	self->disableTimerSending();
}

EXPORTED void controller_destroy(KeyboardController* self) {
	delete self;
}