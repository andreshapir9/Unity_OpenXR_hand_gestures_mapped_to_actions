import numpy as np
import os
import tensorflow as tf
from tensorflow import keras
from tensorflow.keras import layers
import tf2onnx #https://github.com/onnx/tensorflow-onnx
import onnx
from sklearn.utils import shuffle


print("____________________LOADING DATA____________________")
# Load the training data
rock_data = np.load("training_data/rock.npy")
rock_data = rock_data.astype(np.float32)
paper_data = np.load("training_data/paper.npy")
paper_data = paper_data.astype(np.float32)
scissors_data = np.load("training_data/scissors.npy")
scissors_data = scissors_data.astype(np.float32)


x_train = np.concatenate((rock_data, paper_data, scissors_data))
y_train = np.concatenate((np.zeros(len(rock_data)), np.ones(len(paper_data)), np.full(len(scissors_data), 2)))
y_train = keras.utils.to_categorical(y_train, num_classes=3)

#shuffle the data so that the model doesn't learn the order
x_train, y_train = shuffle(x_train, y_train)

#lets split the data into training and test data so we can see how well the model does

#split the data into training and test 70/30
train_size = int(len(x_train) * 0.7)
train_x = x_train[:train_size]
test_x = x_train[train_size:]

#split the labels into training and test 70/30
label_size = int(len(y_train) * 0.7)
label_y = y_train[:label_size]
test_y = y_train[label_size:]

print(x_train.shape)
print(y_train.shape)
print(x_train)
print(y_train)

print(train_x.shape)
print(test_x.shape)


print("____________________TRAINING____________________")

# Create the model
model = keras.Sequential([
    layers.Dense(64, activation='relu', input_shape=[78]),
    layers.Dense(128, activation='relu'),
    layers.Dense(64, activation='relu'),
    layers.Dense(3, activation='softmax')
])

model.compile(optimizer='adam',loss='categorical_crossentropy',metrics=['accuracy'])


model.fit(train_x, label_y, epochs=10, validation_split=0.1)


loss, accuracy = model.evaluate(test_x, test_y)
print("Accuracy: ", accuracy)
print("Loss: ", loss)




print("____________________VALIDATION____________________")

#lets check against another dataset
validation_rocks = np.load("validation_data/rock.npy")
validation_rocks = validation_rocks.astype(np.float32)

validation_papers = np.load("validation_data/paper.npy")
validation_papers = validation_papers.astype(np.float32)

validation_scissors = np.load("validation_data/scissors.npy")
validation_scissors = validation_scissors.astype(np.float32)

x_validation = np.concatenate((validation_rocks, validation_papers, validation_scissors))
y_validation = np.concatenate((np.zeros(len(validation_rocks)), np.ones(len(validation_papers)), np.full(len(validation_scissors), 2)))
y_validation = keras.utils.to_categorical(y_validation, num_classes=3)


print(x_validation.shape)
print(y_validation.shape)
print(x_validation)
print(y_validation)

#lets print the difference between the two



#lets see how many we got right
print("____________________RESULTS____________________")

#testing on the original model
print("Testing on the original model")
# model = keras.models.load_model("models/rock_paper_scissors.h5")
loss, accuracy = model.evaluate(x_validation, y_validation)

print("Accuracy: ", accuracy)
print("Loss: ", loss)
