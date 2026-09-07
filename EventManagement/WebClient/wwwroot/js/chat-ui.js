/**
 * Chat UI Manager
 * Handles the chat interface logic, including loading conversations, sending messages, etc.
 */

class ChatUIManager {
    constructor() {
        this.currentConversationId = null;
        this.currentUserId = this.getCurrentUserId();
        this.currentUsername = this.getCurrentUsername();
        this.chatApiBaseUrl = '/Chats';
        this.typingTimeouts = {};
        this.allConversationIds = [];
    }

    /**
     * Initialize the chat UI
     */
    async initialize() {
        try {
            // Debug: Log the current user ID
            console.log('Chat UI initializing...');
            console.log('Current User ID:', this.currentUserId);
            console.log('Current Username:', this.currentUsername);
            console.log('Chat API Base URL:', this.chatApiBaseUrl);

            const tokenEl = document.getElementById('accessToken');
            if (!tokenEl || !tokenEl.value || tokenEl.value.trim().length === 0) {
                console.warn('AccessToken is missing from current cookie session!');
                this.showAlert('Phiên đăng nhập cũ chưa có Token. Vui lòng Đăng xuất (Logout) và Đăng nhập lại!', 'error');
            }

            if (!this.currentUserId) {
                console.error('User ID is missing! Cannot load conversations.');
                this.showAlert('User not authenticated. Please log in.', 'error');
                return;
            }

            // Load conversations
            await this.loadConversations();

            // Setup event listeners
            this.setupEventListeners();

            // Initialize SignalR if available
            if (typeof chatSignalR !== 'undefined') {
                this.initializeSignalR();
            }

            console.log('Chat UI initialized successfully');
        } catch (error) {
            console.error('Failed to initialize chat UI:', error);
            this.showAlert('Failed to initialize chat', 'error');
        }
    }

    /**
     * Helper to get request headers with Bearer token
     */
    getAuthHeaders() {
        const tokenElement = document.getElementById('accessToken');
        const token = tokenElement ? tokenElement.value : '';
        console.log('[ChatUI] Token length from hidden input:', token ? token.length : 0);
        const headers = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };
        if (token && token.trim().length > 0) {
            headers['Authorization'] = `Bearer ${token.trim()}`;
        } else {
            console.warn('[ChatUI] NO ACCESS TOKEN FOUND IN UI! Authorization header will be missing.');
        }
        return headers;
    }

    /**
     * Load conversations for current user
     */
    async loadConversations() {
        try {
            const url = `${this.chatApiBaseUrl}/conversations/user/${this.currentUserId}`;
            console.log('Loading conversations from:', url);

            const response = await fetch(url, {
                method: 'GET',
                credentials: 'include',  // Include cookies for authentication
                headers: this.getAuthHeaders()
            });

            console.log('Conversations response status:', response.status);
            console.log('Conversations response headers:', response.headers);

            if (!response.ok) {
                const errorText = await response.text();
                console.error('API Error Response:', errorText);
                throw new Error(`Failed to load conversations: ${response.status} ${response.statusText}`);
            }

            const conversations = await response.json();
            console.log('Loaded conversations:', conversations);

            this.conversations = conversations || [];
            // Store all conversation IDs for SignalR group membership
            this.allConversationIds = this.conversations.map(c => c.conversationId);

            this.renderConversations(conversations);
        } catch (error) {
            console.error('Error loading conversations:', error);
            this.showAlert('Failed to load conversations: ' + error.message, 'error');
        }
    }

    /**
     * Render conversations in the panel
     */
    renderConversations(conversations) {
        const conversationsList = document.getElementById('conversationsList');
        conversationsList.innerHTML = '';

        if (!conversations || conversations.length === 0) {
            conversationsList.innerHTML = `
                <div class="no-conversations">
                    <div>
                        <i class="fas fa-inbox fa-2x mb-2"></i>
                        <p>No conversations yet</p>
                    </div>
                </div>
            `;
            return;
        }

        conversations.forEach(conversation => {
            const el = document.createElement('div');
            el.className = `conversation-item ${this.currentConversationId === conversation.conversationId ? 'active' : ''}`;
            const displayTitle = conversation.title || (conversation.type === 'Private' ? 'Direct Message' : 'Event Discussion');
            el.innerHTML = `
                <div class="conversation-title">${displayTitle}</div>
                <div class="conversation-preview">${conversation.messageCount} messages</div>
            `;
            el.onclick = () => this.selectConversation(conversation.conversationId);
            conversationsList.appendChild(el);
        });
    }

    /**
     * Select a conversation and load its messages
     */
    async selectConversation(conversationId) {
        try {
            this.currentConversationId = conversationId;

            // Update UI
            document.querySelectorAll('.conversation-item').forEach(item => {
                item.classList.remove('active');
            });
            event.target.closest('.conversation-item')?.classList.add('active');

            // Load messages
            await this.loadMessages(conversationId);

            // Join SignalR group if connected
            if (typeof chatSignalR !== 'undefined' && chatSignalR.isConnected) {
                await chatSignalR.joinGroup(conversationId, this.currentUserId);
            }
        } catch (error) {
            console.error('Error selecting conversation:', error);
            this.showAlert('Failed to load messages', 'error');
        }
    }

    /**
     * Load messages for a conversation
     */
    async loadMessages(conversationId, pageNumber = 1, pageSize = 50) {
        try {
            const url = `${this.chatApiBaseUrl}/messages/conversation/${conversationId}?pageNumber=${pageNumber}&pageSize=${pageSize}`;
            console.log('Loading messages from:', url);

            const response = await fetch(url, {
                method: 'GET',
                credentials: 'include',
                headers: this.getAuthHeaders()
            });

            console.log('Messages response status:', response.status);

            if (!response.ok) {
                const errorText = await response.text();
                console.error('Failed to load messages. Status:', response.status, 'Response:', errorText);
                throw new Error(`Failed to load messages: ${response.status}`);
            }

            const messages = await response.json();
            console.log('Loaded messages:', messages);
            this.renderMessages(conversationId, messages);
        } catch (error) {
            console.error('Error loading messages:', error);
            this.showAlert('Failed to load messages: ' + error.message, 'error');
        }
    }

    /**
     * Render messages in the chat panel
     */
    renderMessages(conversationId, messages) {
        const messagesPanel = document.getElementById('messagesPanel');
        const currentConv = (this.conversations || []).find(c => c.conversationId === conversationId);
        const headerTitle = currentConv?.title || (currentConv?.type === 'Private' ? 'Direct Message' : 'Event Discussion');

        let html = `
            <div class="messages-header">
                <div class="header-info">
                    <div class="header-title">${headerTitle}</div>
                    <div class="header-subtitle">${messages && messages.length > 0 ? messages.length + ' messages' : 'No messages yet'}</div>
                </div>
                <div class="connection-status">
                    <div class="status-indicator"></div>
                    <span id="statusText">Connected</span>
                </div>
            </div>
            <div class="messages-list">
        `;

        if (!messages || messages.length === 0) {
            html += `
                <div class="empty-state">
                    <div>
                        <i class="fas fa-comments fa-3x mb-3" style="color: #cbd5e1;"></i>
                        <p>No messages yet. Start the conversation!</p>
                    </div>
                </div>
            `;
        } else {
            // Sort messages in chronological order (oldest first)
            // Backend returns them newest first, so we reverse the array
            const sortedMessages = [...messages].reverse();

            sortedMessages.forEach(message => {
                const isOwn = message.senderId === this.currentUserId;
                const messageTime = new Date(message.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
                const senderName = message.senderName || (isOwn ? this.currentUsername : `User ${message.senderId}`);

                html += `
                    <div class="message-group ${isOwn ? 'sent' : 'received'}">
                        <div class="message-sender-info">${senderName}</div>
                        <div class="message-bubble ${isOwn ? 'sent' : 'received'} ${message.isDeleted ? 'deleted' : ''}" data-message-id="${message.messageId}">
                            <div class="message-content">
                                ${message.isDeleted ? '<em>Message was deleted</em>' : message.content}
                            </div>
                            <div class="message-time">${messageTime}</div>
                            ${isOwn && !message.isDeleted ? `
                                <div class="message-actions">
                                    <button onclick="chatUIManager.editMessage(${message.messageId}, '${message.content.replace(/'/g, "\\'")}')">
                                        <i class="fas fa-edit"></i>
                                    </button>
                                    <button onclick="chatUIManager.deleteMessage(${message.messageId})">
                                        <i class="fas fa-trash"></i>
                                    </button>
                                </div>
                            ` : ''}
                        </div>
                    </div>
                `;
            });
        }

        html += `
            </div>
            <div class="message-input-area">
                <div class="input-group">
                    <textarea id="messageInput" class="message-input" placeholder="Type a message..." rows="3"></textarea>
                    <button id="sendBtn" class="send-btn">
                        <i class="fas fa-paper-plane"></i> Send
                    </button>
                </div>
            </div>
        `;

        messagesPanel.innerHTML = html;

        // Setup send button
        document.getElementById('sendBtn').onclick = () => this.sendMessage();
        document.getElementById('messageInput').onkeypress = (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                this.sendMessage();
            }
        };

        // Scroll to bottom
        setTimeout(() => {
            const messagesList = document.querySelector('.messages-list');
            if (messagesList) {
                messagesList.scrollTop = messagesList.scrollHeight;
            }
        }, 0);
    }

    /**
     * Send a message
     */
    async sendMessage() {
        const input = document.getElementById('messageInput');
        const content = input.value.trim();

        if (!this.currentConversationId) {
            this.showAlert('Please select a conversation first.', 'error');
            return;
        }

        if (!content) {
            input.classList.add('is-invalid');
            this.showAlert('Message cannot be empty.', 'error');
            return;
        }

        if (content.length > 2000) {
            input.classList.add('is-invalid');
            this.showAlert('Message cannot exceed 2000 characters.', 'error');
            return;
        }

        input.classList.remove('is-invalid');

        try {
            const response = await fetch(`${this.chatApiBaseUrl}/messages/send`, {
                method: 'POST',
                headers: this.getAuthHeaders(),
                body: JSON.stringify({
                    conversationId: this.currentConversationId,
                    senderId: this.currentUserId,
                    senderName: this.currentUsername,
                    content: content,
                    messageType: 'Text'
                })
            });

            if (!response.ok) {
                throw new Error('Failed to send message');
            }

            // Clear input
            input.value = '';
            input.focus();

            // Reload messages
            await this.loadMessages(this.currentConversationId);
        } catch (error) {
            console.error('Error sending message:', error);
            this.showAlert('Failed to send message', 'error');
        }
    }

    /**
     * Edit a message
     */
    async editMessage(messageId, currentContent) {
        const newContent = prompt('Edit message:', currentContent);
        if (newContent === null) {
            return; // User cancelled
        }

        const trimmed = newContent.trim();
        if (!trimmed) {
            this.showAlert('Message cannot be empty.', 'error');
            return;
        }

        if (trimmed.length > 2000) {
            this.showAlert('Message cannot exceed 2000 characters.', 'error');
            return;
        }

        if (trimmed === currentContent) {
            return; // No change
        }

        try {
            const response = await fetch(`${this.chatApiBaseUrl}/messages/${messageId}`, {
                method: 'PUT',
                headers: this.getAuthHeaders(),
                body: JSON.stringify({
                    content: trimmed
                })
            });

            if (!response.ok) {
                throw new Error('Failed to edit message');
            }

            // Reload messages
            await this.loadMessages(this.currentConversationId);
        } catch (error) {
            console.error('Error editing message:', error);
            this.showAlert('Failed to edit message', 'error');
        }
    }

    /**
     * Delete a message
     */
    async deleteMessage(messageId) {
        if (!confirm('Are you sure you want to delete this message?')) {
            return;
        }

        try {
            const response = await fetch(`${this.chatApiBaseUrl}/messages/${messageId}`, {
                method: 'DELETE',
                headers: this.getAuthHeaders()
            });

            if (!response.ok) {
                throw new Error('Failed to delete message');
            }

            // Reload messages
            await this.loadMessages(this.currentConversationId);
        } catch (error) {
            console.error('Error deleting message:', error);
            this.showAlert('Failed to delete message', 'error');
        }
    }

    /**
     * Initialize SignalR connection
     */
    initializeSignalR() {
        chatSignalR.onMessage((eventType, message) => {
            console.log('Message event:', eventType, message);
            if (eventType === 'received') {
                // Reload conversation list to update message counts
                this.loadConversations();
                if (message.conversationId === this.currentConversationId) {
                    this.loadMessages(this.currentConversationId);
                }
            } else if (eventType === 'edited') {
                if (message.conversationId === this.currentConversationId) {
                    this.loadMessages(this.currentConversationId);
                }
            } else if (eventType === 'deleted') {
                // Reload conversation list to update message counts
                this.loadConversations();
                // Deleted events may not include conversationId, reload current conversation
                if (this.currentConversationId) {
                    this.loadMessages(this.currentConversationId);
                }
            }
        });

        chatSignalR.onStatusChange((status, data) => {
            console.log('Status change:', status, data);
            this.updateConnectionStatus(status);

            // Rejoin all groups after reconnection (SignalR loses group membership on reconnect)
            if (status === 'reconnected' && this.allConversationIds.length > 0) {
                chatSignalR.joinAllGroups(this.allConversationIds);
            }
        });

        chatSignalR.initialize().then(() => {
            // Join all conversation groups so we receive real-time updates for the full list
            if (chatSignalR.isConnected && this.allConversationIds.length > 0) {
                chatSignalR.joinAllGroups(this.allConversationIds);
            }
        }).catch(error => {
            console.error('Failed to initialize SignalR:', error);
        });
    }

    /**
     * Update connection status indicator
     */
    updateConnectionStatus(status) {
        const statusText = document.getElementById('statusText');
        const indicator = document.querySelector('.status-indicator');

        if (!statusText || !indicator) return;

        switch (status) {
            case 'connected':
                statusText.textContent = 'Connected';
                indicator.className = 'status-indicator';
                break;
            case 'disconnected':
                statusText.textContent = 'Disconnected';
                indicator.className = 'status-indicator disconnected';
                break;
            case 'reconnecting':
                statusText.textContent = 'Reconnecting...';
                indicator.className = 'status-indicator disconnected';
                break;
            case 'reconnected':
                statusText.textContent = 'Reconnected';
                indicator.className = 'status-indicator';
                break;
            default:
                statusText.textContent = status;
        }
    }

    /**
     * Setup event listeners
     */
    setupEventListeners() {
        // Add any additional event listeners here
    }

    /**
     * Get current user ID from page or session
     */
    getCurrentUserId() {
        // Try to get from page data attribute
        const userId = document.documentElement.dataset.userId;
        if (userId && !isNaN(userId)) return parseInt(userId);

        // Try to get from hidden input
        const userEl = document.getElementById('currentUserId');
        if (userEl && userEl.value) {
            console.log('Raw userId from hidden input:', userEl.value);

            // Try to parse as integer
            const parsed = parseInt(userEl.value);
            if (!isNaN(parsed)) {
                console.log('Parsed userId as integer:', parsed);
                return parsed;
            }

            // If it's a GUID or UUID, we'll handle it differently
            console.log('UserId appears to be non-numeric:', userEl.value);
            return userEl.value; // Return as string if it's a GUID
        }

        // Try to get from cookie
        const cookies = document.cookie.split(';');
        for (let cookie of cookies) {
            if (cookie.includes('UserId')) {
                const value = cookie.split('=')[1];
                const parsed = parseInt(value);
                if (!isNaN(parsed)) return parsed;
            }
        }

        console.error('Could not find valid user ID from any source');
        return null;
    }

    /**
     * Get current username from page or session
     */
    getCurrentUsername() {
        const username = document.documentElement.dataset.username;
        if (username) return username;

        const userEl = document.getElementById('currentUsername');
        if (userEl) return userEl.value;

        return 'Anonymous';
    }

    /**
     * Show alert message
     */
    showAlert(message, type = 'info') {
        const alertClass = `alert-${type}`;
        const alertHtml = `
            <div class="alert ${alertClass} alert-dismissible fade show" role="alert" style="margin-top: 20px;">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;

        // Add to container if available
        const container = document.getElementById('chatContainer');
        if (container) {
            container.insertAdjacentHTML('beforebegin', alertHtml);
        }
    }
}

// Initialize on page load
let chatUIManager;
document.addEventListener('DOMContentLoaded', async function() {
    chatUIManager = new ChatUIManager();
    await chatUIManager.initialize();
});
