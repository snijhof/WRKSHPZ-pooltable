import cv2
import os

# Define the RTSP stream URL
rtsp_url = "rtsp://192.168.1.124:8554/gopro"

# Open a connection to the RTSP stream
cap = cv2.VideoCapture(rtsp_url)

if not cap.isOpened():
    print("Error: Could not open RTSP stream.")
    exit()

# Create a directory to save the frames
output_dir = "saved_frames"
if not os.path.exists(output_dir):
    os.makedirs(output_dir)

frame_count = 0
saved_frame_count = 0

while True:
    ret, frame = cap.read()
    if not ret:
        print("Failed to grab frame")
        break

    # Display the frame
    cv2.imshow('RTSP Stream', frame)

    # Save every 30th frame to disk
    if frame_count % 30 == 0:
        frame_filename = os.path.join(output_dir, f"frame_{saved_frame_count}.jpg")
        cv2.imwrite(frame_filename, frame)
        print(f"Saved {frame_filename}")
        saved_frame_count += 1

    frame_count += 1

    # Press 'q' to exit the loop
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Release the capture and close any OpenCV windows
cap.release()
cv2.destroyAllWindows()