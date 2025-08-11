import React, { useState } from 'react';
import { View, Text, TextInput, Button, ActivityIndicator, Alert, Image } from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';

export default function Login({ navigation }) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);

  async function handleLogin() {
    if (!username || !password) {
      Alert.alert('Validation', 'Please enter username and password');
      return;
    }
    setLoading(true);
    try {
      await AsyncStorage.setItem('userToken', 'fake-token-123');
      setLoading(false);
      navigation.replace('Home');
    } catch (e) {
      setLoading(false);
      Alert.alert('Login failed', e.message);
    }
  }

  return (
    <View className="flex-1 justify-center p-6 bg-background">
      {/* Image container */}
      <View className="items-center mb-8">
        <Image
          source={require('../assets/login-illustration.png')}
          className="w-48 h-48"
          resizeMode="contain"
        />
      </View>

      {/* Card */}
      <View className="bg-white rounded-xl p-8 shadow-lg">
        <Text className="text-4xl font-extrabold mb-8 text-primary text-center">Welcome Back</Text>
        
        <TextInput
          placeholder="Username"
          className="border border-gray-300 rounded-md px-4 py-3 mb-5"
          autoCapitalize="none"
          value={username}
          onChangeText={setUsername}
          editable={!loading}
        />

        <TextInput
          placeholder="Password"
          className="border border-gray-300 rounded-md px-4 py-3 mb-8"
          secureTextEntry
          value={password}
          onChangeText={setPassword}
          editable={!loading}
        />

        {loading ? (
          <ActivityIndicator size="large" color="#2563EB" />
        ) : (
          <Button color="#2563EB" title="Log In" onPress={handleLogin} />
        )}
      </View>
    </View>
  );
}
