import numpy as np
import os

# our data looks like this: 
# {
    # "name": {
    # 		"x": 0,
    # 		"y": 0,
    # 		"z": 0
    # 	},
    # "name": {
    # 		"x": 0,
    # 		"y": 0,
    # 		"z": 0
    # 	},
    # ...
# }
# we want to convert it to this:
# [x1,y1,z1,x2,y2,z2...xn,yn,zn]

class Joint:
    def __init__(self, name, x, y, z):
        self.name = name
        self.x = x
        self.y = y
        self.z = z
    def get_position(self):
        return [self.x, self.y, self.z]
    def __str__(self):
        return self.name + " [ " + self.x + ", " + self.y + ", " + self.z + " ]"

class Hand:
    def __init__(self, joints):
        self.joints = joints
    def get_positions(self):
        positions = []
        for joint in self.joints:
            positions.extend(joint.get_position())
        return positions
    def __str__(self):
        joint_string = "[ "
        for joint in self.joints:
            joint_string += str(joint) + ", "
        joint_string += " ]"
        return joint_string
    def __repr__(self):
        return self.get_positions()
    

#takes in string data and returns a list of joints
def serialize_data(data):
    #lets remove all whitespace
    hand = []
    countLines = 0
    countValues = 0
    joints = data.split("},")
    for i in range(len(joints)):
        name = joints[i].split(":")[0].replace("{", "").replace('"', "").replace("\n", "").replace("\t", "")
        x = joints[i].split(",")[0].split(":")[2].replace("\n", "").replace("\t", "").replace("   ", "")
        y = joints[i].split(",")[1].split(":")[1].replace("\n", "").replace("\t", "")
        z = joints[i].split(",")[2].split(":")[1].replace("}", "").replace("\n", "").replace("\t", "")
        hand.append(Joint(name, x, y, z))
        countLines += 1
        countValues += 3

    # print("countLines: ", countLines)
    # print("countValues: ", countValues)
    return Hand(hand)
            
   
def read_data():
    #opens a directory and gets a list of all the files in it
    files = os.listdir("validation_data/paper")
    gestures = []
    #opens a file and gets a string of all the lines in it
    for file in files:
        
        #we open each file in the directory
        f = open("validation_data/paper/" + file, "r")
        file_data = f.readlines()
        file_as_string = ""
        for line in file_data:
            file_as_string += line
        f.close()
        gestures.append(serialize_data(file_as_string))
    return gestures
        
#save data as a numpy array
def save_data(data):
    data_as_double = []
    for gesture in data:
        gesture_as_double = []
        for joint in gesture.joints:
            gesture_as_double.append(float(joint.x))
            gesture_as_double.append(float(joint.y))
            gesture_as_double.append(float(joint.z))
        data_as_double.append(gesture_as_double)
    np.save("validation_data/paper.npy", data_as_double)

# print(read_data())
save_data(read_data())

#lets load the data and see what it looks like
data = np.load("validation_data/paper.npy")
data = data.astype(np.float32)
np.save("validation_data/paper.npy", data)
print("data: ", data)
print("data shape: ", data.shape)