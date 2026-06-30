import 'package:flutter/material.dart';
import '../models/event_model.dart';
import '../services/api_service.dart';

class EventProvider extends ChangeNotifier {
  final ApiService _apiService;

  bool _isLoading = false;
  List<EventModel> _events = [];
  String? _errorMessage;

  EventProvider({ApiService? apiService})
      : _apiService = apiService ?? ApiService();

  bool get isLoading => _isLoading;
  List<EventModel> get events => _events;
  String? get errorMessage => _errorMessage;

  void _setLoading(bool value) {
    _isLoading = value;
    notifyListeners();
  }

  Future<void> fetchEvents() async {
    _setLoading(true);
    _errorMessage = null;
    try {
      _events = await _apiService.getAllEvents();
    } catch (e) {
      _errorMessage = e.toString();
      _events = [];
    } finally {
      _setLoading(false);
    }
  }

  Future<void> createEvent(Map<String, dynamic> data) async {
    _setLoading(true);
    _errorMessage = null;
    try {
      final newEvent = await _apiService.createEvent(data);
      _events.add(newEvent);
      notifyListeners();
    } catch (e) {
      _errorMessage = e.toString();
      rethrow;
    } finally {
      _setLoading(false);
    }
  }

  Future<void> updateEvent(int id, Map<String, dynamic> data) async {
    _setLoading(true);
    _errorMessage = null;
    try {
      final updatedEvent = await _apiService.updateEvent(id, data);
      final index = _events.indexWhere((e) => e.id == id);
      if (index != -1) {
        _events[index] = updatedEvent;
      }
      notifyListeners();
    } catch (e) {
      _errorMessage = e.toString();
      rethrow;
    } finally {
      _setLoading(false);
    }
  }

  Future<void> deleteEvent(int id) async {
    _setLoading(true);
    _errorMessage = null;
    try {
      await _apiService.deleteEvent(id);
      _events.removeWhere((e) => e.id == id);
      notifyListeners();
    } catch (e) {
      _errorMessage = e.toString();
      rethrow;
    } finally {
      _setLoading(false);
    }
  }
}
