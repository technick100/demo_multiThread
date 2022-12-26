import threading
import time
import random
from datetime import datetime

queue = []
thread_count = 2

def producer(_thread_id):
    print("%s:Producer %d, begin\n" %
                          (datetime.now().strftime('%Y-%m-%d %H:%M:%S.%f')[:-3], _thread_id))
    time.sleep(0.1)
    condition_obj = threading.Condition()
    # Creating r number of items by the producer
    for i in range(1, 2):
        r = random.randint(0, 100)
        print("%s:Producer %d, generated number: %d" %
                          (datetime.now().strftime('%Y-%m-%d %H:%M:%S.%f')[:-3], _thread_id, r))
        condition_obj.acquire()
        try:
            # Notify that an item  has been produced
            print("%s:Producer %d, locked" %
                          (datetime.now().strftime('%Y-%m-%d %H:%M:%S.%f')[:-3], _thread_id))
            queue.append({ "thread_id": 1, "condition": condition_obj, "input":  r })
            condition_obj.wait()
        finally:
            # Releasing the lock after producing
            condition_obj.release()
            print("%s:Producer %d, released" %
                          (datetime.now().strftime('%Y-%m-%d %H:%M:%S.%f')[:-3], _thread_id))
            print("%s:Producer %d,  end: %d" %
                          (datetime.now().strftime('%Y-%m-%d %H:%M:%S.%f')[:-3], _thread_id, r))

def consumer():    
    while True:
        if len(queue) > 0:
            item = queue.pop(0)
            if item is not None and "condition" in item:
                condition = item["condition"]
                condition.acquire()
                condition.notify()
                condition.release()  
        time.sleep(0.1)

def main() -> None:
    con = threading.Thread(target=consumer, args=())
    con.start()  
    
    time.sleep(1)    
    producers = []
    for i in range(thread_count):
        p = threading.Thread(target=producer, args=(i,))
        producers.append(p)
    for x in producers:
        x.start()

if __name__ == "__main__":
    main()