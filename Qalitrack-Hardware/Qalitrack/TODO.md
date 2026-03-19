# Camera Enabling TODO

## Plan Steps
1. [x] **Edit config**: Fixed RtspPath for npr1 to "/stream1", added npr2 camera.
2. [ ] **Restart service/app**: Apply changes (dotnet run or service restart).
3. [ ] **Verify /api/Camera/cameras**: Shows 2 cameras.
4. [ ] **Test status**: curl /api/Camera/npr1/status → connected/healthy.
5. [ ] **Test stream**: Browser /api/Camera/npr1/stream shows video.
6. [ ] **Monitor logs**: FFmpeg connections successful.
7. [ ] Complete: attempt_completion.

Current: Starting step 1.
