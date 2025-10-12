import time
from datetime import datetime

while True:
    current_time = datetime.now().strftime("%H:%M:%S.%f")[:-3]
    print(current_time)
    time.sleep(0.001)
