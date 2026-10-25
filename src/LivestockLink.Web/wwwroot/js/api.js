/**
 * LivestockLink Nigeria - API Client & Utility Functions
 */
const API = {
  baseUrl: window.location.origin,

  getToken() {
    return localStorage.getItem('ll_token');
  },

  setToken(token) {
    if (token) {
      localStorage.setItem('ll_token', token);
    } else {
      localStorage.removeItem('ll_token');
    }
  },

  getCurrentUser() {
    const userStr = localStorage.getItem('ll_user');
    return userStr ? JSON.parse(userStr) : null;
  },

  setCurrentUser(user) {
    if (user) {
      localStorage.setItem('ll_user', JSON.stringify(user));
    } else {
      localStorage.removeItem('ll_user');
    }
  },

  logout() {
    this.setToken(null);
    this.setCurrentUser(null);
    window.location.href = '/index.html';
  },

  async request(endpoint, options = {}) {
    const url = `${this.baseUrl}${endpoint}`;
    const headers = {
      'Content-Type': 'application/json',
      ...options.headers
    };

    const token = this.getToken();
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    try {
      const response = await fetch(url, { ...options, headers });
      
      if (response.status === 204) {
        return null;
      }

      const data = await response.json().catch(() => null);

      if (!response.ok) {
        if (response.status === 401) {
          // Token expired or invalid
          console.warn('Unauthorized request, logging out...');
        }
        const errorMsg = data?.error || data?.title || (data?.errors ? Object.values(data.errors).flat().join(' ') : 'Request failed.');
        throw new Error(errorMsg);
      }

      return data;
    } catch (err) {
      throw err;
    }
  },

  get(endpoint) {
    return this.request(endpoint, { method: 'GET' });
  },

  post(endpoint, body) {
    return this.request(endpoint, {
      method: 'POST',
      body: JSON.stringify(body)
    });
  },

  put(endpoint, body) {
    return this.request(endpoint, {
      method: 'PUT',
      body: JSON.stringify(body)
    });
  },

  patch(endpoint, body) {
    return this.request(endpoint, {
      method: 'PATCH',
      body: body ? JSON.stringify(body) : undefined
    });
  },

  delete(endpoint) {
    return this.request(endpoint, { method: 'DELETE' });
  },

  // Nigerian Naira Currency Formatter
  formatNaira(amount) {
    if (amount === undefined || amount === null || isNaN(amount)) return '₦0.00';
    return new Intl.NumberFormat('en-NG', {
      style: 'currency',
      currency: 'NGN',
      currencyDisplay: 'narrowSymbol'
    }).format(amount).replace('NGN', '₦');
  },

  // Date Formatter
  formatDate(dateString) {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB', {
      day: 'numeric',
      month: 'short',
      year: 'numeric'
    });
  },

  formatDateTime(dateString) {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleString('en-GB', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
};
