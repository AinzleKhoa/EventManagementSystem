/**
 * Chat SignalR Real-Time Integration
 * Handles WebSocket connection to ChatAPI Hub
 */

class ChatSignalR {
    constructor() {
        this.connection = null;
        this.currentGroupId = null;
        this.isConnected = false;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
        this.reconnectDelay = 3000;
        this.messageCallbacks = [];
        this.statusCallbacks = [];
    }

    /**
     * Initialize SignalR connection
    async initialize(chatApiUrl = null) {
        try {
            if (!chatApiUrl) {
                chatApiUrl = 'https://localhost:7148';
            }
            console.log('Initializing SignalR connection to:', chatApiUrl);

            const tokenElement = document.getElementById('accessToken');
            const token = tokenElement ? tokenElement.value : '';

            this.connection = new signalR.HubConnectionBuilder()
                .withUrl(`${chatApiUrl}/hubs/chat`, {
                    accessTokenFactory: () => {
                        const tokenEl = document.getElementById('accessToken');
                        const tokenVal = tokenEl && tokenEl.value ? tokenEl.value : token;
                        console.log('[SignalR] Token length:', tokenVal ? tokenVal.length : 0);
                        return tokenVal;
                    },
                    withCredentials: true,  // Important: Send credentials
                    skipNegotiation: false,
                    transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
                })
                .withAutomaticReconnect([0, 0, 0, 3000, 5000, 10000])
                .withHubProtocol(new signalR.JsonHubProtocol())
                .configureLogging(signalR.LogLevel.Information)
                .build();

            // Event handlers
            this.connection.on("MessageReceived", (message) => this.onMessageReceived(message));
            this.connection.on("MessageEdited", (message) => this.onMessageEdited(message));
            this.connection.on("MessageDeleted", (messageId) => this.onMessageDeleted(messageId));
            this.connection.on("UserJoined", (userId, username) => this.onUserJoined(userId, username));
            this.connection.on("UserLeft", (userId, username) => this.onUserLeft(userId, username));
            this.connection.on("UserTyping", (userId, username) => this.onUserTyping(userId, username));

            // Connection events
            this.connection.onreconnecting((error) => this.onReconnecting(error));
            this.connection.onreconnected((connectionId) => this.onReconnected(connectionId));
            this.connection.onclose((error) => this.onConnectionClosed(error));

            // Start connection
            await this.connection.start();
            this.isConnected = true;
            this.reconnectAttempts = 0;
            this.notifyStatusChange('connected', 'Connected to chat server');
            console.log('SignalR connected to chat hub');

        } catch (error) {
            console.error('Failed to connect to SignalR:', error);
            this.notifyStatusChange('error', 'Failed to connect to chat server');
        }
    }

    /**
     * Join a chat group
     * @param {number} conversationId - ID of the conversation
     * @param {number} userId - ID of the user
     */
    async joinGroup(conversationId, userId) {
        if (!this.isConnected) {
            console.warn('Not connected to SignalR');
            return;
        }

        try {
            this.currentGroupId = conversationId;
            await this.connection.invoke("JoinConversation", conversationId);
            this.notifyStatusChange('joined', `Joined conversation ${conversationId}`);
            console.log(`Joined conversation ${conversationId}`);
        } catch (error) {
            console.error('Error joining group:', error);
            this.notifyStatusChange('error', 'Failed to join conversation');
        }
    }

    /**
     * Join all conversation groups at once (for real-time updates on conversation list)
     * @param {number[]} conversationIds - Array of conversation IDs
     */
    async joinAllGroups(conversationIds) {
        if (!this.isConnected) {
            console.warn('Not connected to SignalR');
            return;
        }

        try {
            await this.connection.invoke("JoinAllConversations", conversationIds);
            console.log(`Joined ${conversationIds.length} conversation groups`);
        } catch (error) {
            console.error('Error joining all groups:', error);
        }
    }

    /**
     * Leave a chat group
     * @param {number} conversationId - ID of the conversation
     * @param {number} userId - ID of the user
     */
    async leaveGroup(conversationId, userId) {
        if (!this.isConnected) {
            return;
        }

        try {
            await this.connection.invoke("LeaveConversation", conversationId);
            this.currentGroupId = null;
            console.log(`Left conversation ${conversationId}`);
        } catch (error) {
            console.error('Error leaving group:', error);
        }
    }

    /**
     * Send a message
     * @param {number} conversationId - ID of the conversation
     * @param {number} senderId - ID of the sender
     * @param {string} content - Message content
     */
    async sendMessage(conversationId, senderId, content) {
        if (!this.isConnected) {
            console.warn('Not connected to SignalR');
            return;
        }

        try {
            await this.connection.invoke("SendMessage", conversationId, senderId, content);
        } catch (error) {
            console.error('Error sending message:', error);
            this.notifyStatusChange('error', 'Failed to send message');
        }
    }

    /**
     * Broadcast typing indicator
     * @param {number} conversationId - ID of the conversation
     * @param {number} userId - ID of the user
     * @param {string} username - Username
     */
    async notifyTyping(conversationId, userId, username) {
        if (!this.isConnected) {
            return;
        }

        try {
            await this.connection.invoke("UserTyping", conversationId, userId, username);
        } catch (error) {
            console.error('Error sending typing notification:', error);
        }
    }

    /**
     * Handle incoming message
     * @param {object} message - Message object
     */
    onMessageReceived(message) {
        console.log('Message received:', message);
        this.notifyMessageCallbacks('received', message);
    }

    /**
     * Handle edited message
     * @param {object} message - Edited message object
     */
    onMessageEdited(message) {
        console.log('Message edited:', message);
        this.notifyMessageCallbacks('edited', message);
    }

    /**
     * Handle deleted message
     * @param {number} messageId - ID of deleted message
     */
    onMessageDeleted(messageId) {
        console.log('Message deleted:', messageId);
        this.notifyMessageCallbacks('deleted', { messageId });
    }

    /**
     * Handle user joined
     */
    onUserJoined(userId, username) {
        console.log(`User joined: ${username}`);
        this.notifyStatusChange('userJoined', `${username} joined the conversation`);
    }

    /**
     * Handle user left
     */
    onUserLeft(userId, username) {
        console.log(`User left: ${username}`);
        this.notifyStatusChange('userLeft', `${username} left the conversation`);
    }

    /**
     * Handle user typing
     */
    onUserTyping(userId, username) {
        console.log(`User typing: ${username}`);
        this.notifyStatusChange('userTyping', { userId, username });
    }

    /**
     * Handle reconnecting
     */
    onReconnecting(error) {
        this.isConnected = false;
        this.reconnectAttempts++;
        console.warn(`Reconnecting... Attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts}`);
        this.notifyStatusChange('reconnecting', `Attempting to reconnect (${this.reconnectAttempts}/${this.maxReconnectAttempts})`);
    }

    /**
     * Handle reconnected
     */
    onReconnected(connectionId) {
        this.isConnected = true;
        this.reconnectAttempts = 0;
        console.log('Reconnected to SignalR:', connectionId);
        this.notifyStatusChange('reconnected', 'Reconnected to chat server');
    }

    /**
     * Handle connection closed
     */
    onConnectionClosed(error) {
        this.isConnected = false;
        console.error('Connection closed:', error);

        if (this.reconnectAttempts < this.maxReconnectAttempts) {
            setTimeout(() => {
                console.log('Attempting to reconnect...');
                this.initialize().catch(err => console.error('Reconnection failed:', err));
            }, this.reconnectDelay);
        } else {
            this.notifyStatusChange('disconnected', 'Failed to reconnect to chat server');
        }
    }

    /**
     * Register callback for message events
     * @param {function} callback - Callback function(eventType, message)
     */
    onMessage(callback) {
        this.messageCallbacks.push(callback);
    }

    /**
     * Register callback for status events
     * @param {function} callback - Callback function(status, data)
     */
    onStatusChange(callback) {
        this.statusCallbacks.push(callback);
    }

    /**
     * Notify all message callbacks
     */
    notifyMessageCallbacks(eventType, message) {
        this.messageCallbacks.forEach(callback => {
            try {
                callback(eventType, message);
            } catch (error) {
                console.error('Error in message callback:', error);
            }
        });
    }

    /**
     * Notify all status callbacks
     */
    notifyStatusChange(status, data) {
        this.statusCallbacks.forEach(callback => {
            try {
                callback(status, data);
            } catch (error) {
                console.error('Error in status callback:', error);
            }
        });
    }

    /**
     * Disconnect from SignalR
     */
    async disconnect() {
        if (this.connection) {
            try {
                await this.connection.stop();
                this.isConnected = false;
                console.log('Disconnected from SignalR');
            } catch (error) {
                console.error('Error disconnecting:', error);
            }
        }
    }
}

// Global instance
const chatSignalR = new ChatSignalR();
