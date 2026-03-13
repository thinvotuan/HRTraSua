"""Tests for the Thealley HR System."""
import pytest
from app import create_app, db as _db
from app.models import Department, Position, Employee, AttendanceRecord, LeaveRequest, PayrollRecord
from datetime import date


@pytest.fixture
def app():
    """Create application for testing."""
    app = create_app({
        'TESTING': True,
        'SQLALCHEMY_DATABASE_URI': 'sqlite:///:memory:',
        'WTF_CSRF_ENABLED': False,
        'SECRET_KEY': 'test-secret',
    })
    with app.app_context():
        _db.create_all()
        yield app
        _db.session.remove()
        _db.drop_all()


@pytest.fixture
def client(app):
    return app.test_client()


@pytest.fixture
def dept(app):
    with app.app_context():
        d = Department(name='Test Dept', description='Test')
        _db.session.add(d)
        _db.session.commit()
        return _db.session.get(Department, d.id)


@pytest.fixture
def position(app, dept):
    with app.app_context():
        p = Position(name='Test Position', department_id=dept.id, base_salary=8000000)
        _db.session.add(p)
        _db.session.commit()
        return _db.session.get(Position, p.id)


@pytest.fixture
def employee(app, dept, position):
    with app.app_context():
        e = Employee(
            employee_code='T001',
            full_name='Nguyễn Test',
            department_id=dept.id,
            position_id=position.id,
            hire_date=date(2022, 1, 1),
            status='active',
        )
        _db.session.add(e)
        _db.session.commit()
        return _db.session.get(Employee, e.id)


# ---- Dashboard ----

def test_index_page(client):
    resp = client.get('/')
    assert resp.status_code == 200
    assert 'Tổng quan'.encode() in resp.data


# ---- Departments ----

def test_departments_list(client):
    resp = client.get('/phong-ban/')
    assert resp.status_code == 200


def test_create_department(client, app):
    resp = client.post('/phong-ban/them', data={
        'name': 'Phòng Kỹ Thuật',
        'description': 'Bộ phận kỹ thuật',
    }, follow_redirects=True)
    assert resp.status_code == 200
    with app.app_context():
        assert Department.query.filter_by(name='Phòng Kỹ Thuật').first() is not None


def test_create_department_duplicate(client, app, dept):
    resp = client.post('/phong-ban/them', data={
        'name': 'Test Dept',
        'description': '',
    }, follow_redirects=True)
    assert resp.status_code == 200
    assert 'đã tồn tại'.encode() in resp.data


def test_update_department(client, app, dept):
    resp = client.post(f'/phong-ban/{dept.id}/sua', data={
        'name': 'Updated Dept',
        'description': 'Updated',
    }, follow_redirects=True)
    assert resp.status_code == 200
    with app.app_context():
        updated = _db.session.get(Department, dept.id)
        assert updated.name == 'Updated Dept'


def test_delete_department_with_employees_blocked(client, app, employee):
    with app.app_context():
        emp = _db.session.get(Employee, employee.id)
        dept_id = emp.department_id
    resp = client.post(f'/phong-ban/{dept_id}/xoa', follow_redirects=True)
    assert resp.status_code == 200
    assert 'Không thể xóa'.encode() in resp.data


# ---- Positions ----

def test_positions_list(client):
    resp = client.get('/chuc-vu/')
    assert resp.status_code == 200


def test_create_position(client, app, dept):
    resp = client.post('/chuc-vu/them', data={
        'name': 'Kỹ Sư',
        'department_id': str(dept.id),
        'base_salary': '10000000',
    }, follow_redirects=True)
    assert resp.status_code == 200
    with app.app_context():
        assert Position.query.filter_by(name='Kỹ Sư').first() is not None


def test_update_position(client, app, position):
    resp = client.post(f'/chuc-vu/{position.id}/sua', data={
        'name': 'Senior Position',
        'department_id': str(position.department_id),
        'base_salary': '12000000',
    }, follow_redirects=True)
    assert resp.status_code == 200
    with app.app_context():
        updated = _db.session.get(Position, position.id)
        assert updated.base_salary == 12000000


# ---- Employees ----

def test_employees_list(client):
    resp = client.get('/nhan-vien/')
    assert resp.status_code == 200


def test_create_employee(client, app, dept, position):
    resp = client.post('/nhan-vien/them', data={
        'employee_code': 'E999',
        'full_name': 'Lê Văn Test',
        'gender': 'Nam',
        'date_of_birth': '1995-01-01',
        'phone': '0900000001',
        'email': 'test@thealley.vn',
        'address': 'Test Address',
        'department_id': str(dept.id),
        'position_id': str(position.id),
        'hire_date': '2023-01-01',
        'status': 'active',
    }, follow_redirects=True)
    assert resp.status_code == 200
    with app.app_context():
        assert Employee.query.filter_by(employee_code='E999').first() is not None


def test_create_employee_duplicate_code(client, app, employee):
    with app.app_context():
        emp = _db.session.get(Employee, employee.id)
        dept_id = emp.department_id
        pos_id = emp.position_id
    resp = client.post('/nhan-vien/them', data={
        'employee_code': 'T001',
        'full_name': 'Duplicate',
        'department_id': str(dept_id),
        'position_id': str(pos_id),
        'hire_date': '2023-01-01',
        'status': 'active',
    }, follow_redirects=True)
    assert resp.status_code == 200
    assert 'đã tồn tại'.encode() in resp.data


def test_employee_detail(client, app, employee):
    resp = client.get(f'/nhan-vien/{employee.id}')
    assert resp.status_code == 200
    assert 'Nguyễn Test'.encode() in resp.data


def test_update_employee(client, app, employee, dept, position):
    resp = client.post(f'/nhan-vien/{employee.id}/sua', data={
        'full_name': 'Nguyễn Updated',
        'gender': 'Nam',
        'phone': '0912345678',
        'email': 'updated@thealley.vn',
        'address': 'New Address',
        'department_id': str(dept.id),
        'position_id': str(position.id),
        'hire_date': '2022-01-01',
        'status': 'active',
    }, follow_redirects=True)
    assert resp.status_code == 200
    with app.app_context():
        updated = _db.session.get(Employee, employee.id)
        assert updated.full_name == 'Nguyễn Updated'


def test_delete_employee(client, app, employee):
    resp = client.post(f'/nhan-vien/{employee.id}/xoa', follow_redirects=True)
    assert resp.status_code == 200
    with app.app_context():
        assert _db.session.get(Employee, employee.id) is None


def test_employee_search(client, app, employee):
    resp = client.get('/nhan-vien/?q=Nguyễn Test')
    assert resp.status_code == 200


# ---- Attendance ----

def test_attendance_index(client):
    resp = client.get('/cham-cong/')
    assert resp.status_code == 200


def test_create_attendance(client, app, employee):
    resp = client.post('/cham-cong/them', data={
        'employee_id': str(employee.id),
        'date': '2024-03-01',
        'check_in': '08:00',
        'check_out': '17:00',
        'status': 'present',
        'note': '',
    }, follow_redirects=True)
    assert resp.status_code == 200
    with app.app_context():
        rec = AttendanceRecord.query.filter_by(
            employee_id=employee.id, date=date(2024, 3, 1)
        ).first()
        assert rec is not None
        assert rec.status == 'present'


def test_attendance_duplicate_blocked(client, app, employee):
    # First record
    client.post('/cham-cong/them', data={
        'employee_id': str(employee.id),
        'date': '2024-03-01',
        'check_in': '08:00',
        'check_out': '17:00',
        'status': 'present',
    }, follow_redirects=True)
    # Duplicate
    resp = client.post('/cham-cong/them', data={
        'employee_id': str(employee.id),
        'date': '2024-03-01',
        'check_in': '08:00',
        'check_out': '17:00',
        'status': 'present',
    }, follow_redirects=True)
    assert resp.status_code == 200
    assert 'Đã có bản ghi'.encode() in resp.data


def test_attendance_report(client):
    resp = client.get('/cham-cong/bao-cao')
    assert resp.status_code == 200


def test_bulk_attendance_save(client, app, employee):
    resp = client.post('/cham-cong/hang-loat', data={
        'date': '2024-03-02',
        'employee_ids': [str(employee.id)],
        f'status_{employee.id}': 'present',
        f'check_in_{employee.id}': '08:30',
        f'check_out_{employee.id}': '17:30',
    }, follow_redirects=True)
    assert resp.status_code == 200
    with app.app_context():
        rec = AttendanceRecord.query.filter_by(
            employee_id=employee.id, date=date(2024, 3, 2)
        ).first()
        assert rec is not None


# ---- Leave Requests ----

def test_leaves_list(client):
    resp = client.get('/nghi-phep/')
    assert resp.status_code == 200


def test_create_leave(client, app, employee):
    resp = client.post('/nghi-phep/them', data={
        'employee_id': str(employee.id),
        'leave_type': 'annual',
        'start_date': '2024-03-10',
        'end_date': '2024-03-12',
        'reason': 'Du lịch gia đình',
    }, follow_redirects=True)
    assert resp.status_code == 200
    with app.app_context():
        leave = LeaveRequest.query.filter_by(employee_id=employee.id).first()
        assert leave is not None
        assert leave.status == 'pending'
        assert leave.days_count == 3


def test_create_leave_invalid_dates(client, app, employee):
    resp = client.post('/nghi-phep/them', data={
        'employee_id': str(employee.id),
        'leave_type': 'annual',
        'start_date': '2024-03-12',
        'end_date': '2024-03-10',
        'reason': '',
    }, follow_redirects=True)
    assert resp.status_code == 200
    assert 'sau ngày bắt đầu'.encode() in resp.data


def test_approve_leave(client, app, employee):
    with app.app_context():
        leave = LeaveRequest(
            employee_id=employee.id,
            leave_type='sick',
            start_date=date(2024, 3, 5),
            end_date=date(2024, 3, 6),
            status='pending',
        )
        _db.session.add(leave)
        _db.session.commit()
        leave_id = leave.id

    resp = client.post(f'/nghi-phep/{leave_id}/duyet', data={
        'action': 'approve',
        'approved_by': 'Manager',
    }, follow_redirects=True)
    assert resp.status_code == 200
    with app.app_context():
        updated = _db.session.get(LeaveRequest, leave_id)
        assert updated.status == 'approved'


def test_reject_leave(client, app, employee):
    with app.app_context():
        leave = LeaveRequest(
            employee_id=employee.id,
            leave_type='personal',
            start_date=date(2024, 3, 7),
            end_date=date(2024, 3, 7),
            status='pending',
        )
        _db.session.add(leave)
        _db.session.commit()
        leave_id = leave.id

    resp = client.post(f'/nghi-phep/{leave_id}/duyet', data={
        'action': 'reject',
        'approved_by': 'Manager',
    }, follow_redirects=True)
    assert resp.status_code == 200
    with app.app_context():
        updated = _db.session.get(LeaveRequest, leave_id)
        assert updated.status == 'rejected'


# ---- Payroll ----

def test_payroll_index(client):
    resp = client.get('/luong/')
    assert resp.status_code == 200


def test_generate_payroll(client, app, employee):
    resp = client.post('/luong/tao-bang-luong', data={
        'month': '3',
        'year': '2024',
    }, follow_redirects=True)
    assert resp.status_code == 200
    with app.app_context():
        rec = PayrollRecord.query.filter_by(
            employee_id=employee.id, month=3, year=2024
        ).first()
        assert rec is not None


def test_update_payroll(client, app, employee):
    with app.app_context():
        rec = PayrollRecord(
            employee_id=employee.id,
            month=4,
            year=2024,
            base_salary=8000000,
            work_days=20,
            standard_days=26,
            status='draft',
        )
        _db.session.add(rec)
        _db.session.commit()
        rec_id = rec.id

    resp = client.post(f'/luong/{rec_id}/sua', data={
        'work_days': '22',
        'standard_days': '26',
        'allowance': '500000',
        'bonus': '1000000',
        'deduction': '200000',
        'note': 'Test note',
        'status': 'draft',
    }, follow_redirects=True)
    assert resp.status_code == 200
    with app.app_context():
        updated = _db.session.get(PayrollRecord, rec_id)
        assert updated.work_days == 22
        assert updated.allowance == 500000
        assert updated.bonus == 1000000


def test_mark_payroll_paid(client, app, employee):
    with app.app_context():
        rec = PayrollRecord(
            employee_id=employee.id,
            month=5,
            year=2024,
            base_salary=8000000,
            work_days=22,
            standard_days=26,
            status='draft',
        )
        _db.session.add(rec)
        _db.session.commit()

    resp = client.post('/luong/xac-nhan-thanh-toan', data={
        'month': '5',
        'year': '2024',
    }, follow_redirects=True)
    assert resp.status_code == 200
    with app.app_context():
        updated = PayrollRecord.query.filter_by(
            employee_id=employee.id, month=5, year=2024
        ).first()
        assert updated.status == 'paid'


# ---- Model Tests ----

def test_attendance_work_hours(app, employee):
    with app.app_context():
        rec = AttendanceRecord(
            employee_id=employee.id,
            date=date(2024, 3, 1),
            check_in='08:00',
            check_out='17:00',
            status='present',
        )
        assert rec.work_hours == 9.0


def test_payroll_net_salary(app, employee):
    with app.app_context():
        rec = PayrollRecord(
            employee_id=employee.id,
            month=3,
            year=2024,
            base_salary=10000000,
            allowance=500000,
            bonus=1000000,
            deduction=500000,
            work_days=26,
            standard_days=26,
        )
        assert rec.actual_salary == 10000000
        assert rec.gross_salary == 11500000
        assert rec.net_salary == 11000000


def test_leave_days_count(app, employee):
    with app.app_context():
        leave = LeaveRequest(
            employee_id=employee.id,
            leave_type='annual',
            start_date=date(2024, 3, 1),
            end_date=date(2024, 3, 5),
        )
        assert leave.days_count == 5


def test_employee_age(app, employee):
    with app.app_context():
        emp = _db.session.get(Employee, employee.id)
        dob = date(1995, 1, 1)
        emp.date_of_birth = dob
        expected_age = (date.today() - dob).days // 365
        assert emp.age is not None
        assert emp.age == expected_age or emp.age == expected_age + 1
