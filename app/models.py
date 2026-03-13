from app import db
from datetime import date


class Department(db.Model):
    __tablename__ = 'departments'

    id = db.Column(db.Integer, primary_key=True)
    name = db.Column(db.String(100), nullable=False, unique=True)
    description = db.Column(db.String(255))
    created_at = db.Column(db.DateTime, default=db.func.now())

    positions = db.relationship('Position', backref='department', lazy=True,
                                cascade='all, delete-orphan')
    employees = db.relationship('Employee', backref='department', lazy=True)

    def __repr__(self):
        return f'<Department {self.name}>'

    @property
    def employee_count(self):
        return Employee.query.filter_by(department_id=self.id, status='active').count()


class Position(db.Model):
    __tablename__ = 'positions'

    id = db.Column(db.Integer, primary_key=True)
    name = db.Column(db.String(100), nullable=False)
    department_id = db.Column(db.Integer, db.ForeignKey('departments.id'), nullable=False)
    base_salary = db.Column(db.Float, default=0)
    created_at = db.Column(db.DateTime, default=db.func.now())

    employees = db.relationship('Employee', backref='position', lazy=True)

    def __repr__(self):
        return f'<Position {self.name}>'


class Employee(db.Model):
    __tablename__ = 'employees'

    id = db.Column(db.Integer, primary_key=True)
    employee_code = db.Column(db.String(20), nullable=False, unique=True)
    full_name = db.Column(db.String(150), nullable=False)
    date_of_birth = db.Column(db.Date)
    gender = db.Column(db.String(10))
    phone = db.Column(db.String(20))
    email = db.Column(db.String(150))
    address = db.Column(db.String(255))
    department_id = db.Column(db.Integer, db.ForeignKey('departments.id'))
    position_id = db.Column(db.Integer, db.ForeignKey('positions.id'))
    hire_date = db.Column(db.Date, default=date.today)
    status = db.Column(db.String(20), default='active')  # active, inactive
    created_at = db.Column(db.DateTime, default=db.func.now())

    attendance_records = db.relationship('AttendanceRecord', backref='employee',
                                         lazy=True, cascade='all, delete-orphan')
    leave_requests = db.relationship('LeaveRequest', backref='employee',
                                     lazy=True, cascade='all, delete-orphan')
    payroll_records = db.relationship('PayrollRecord', backref='employee',
                                      lazy=True, cascade='all, delete-orphan')

    def __repr__(self):
        return f'<Employee {self.employee_code} - {self.full_name}>'

    @property
    def age(self):
        if self.date_of_birth:
            today = date.today()
            return today.year - self.date_of_birth.year - (
                (today.month, today.day) < (self.date_of_birth.month, self.date_of_birth.day)
            )
        return None

    @property
    def years_of_service(self):
        if self.hire_date:
            today = date.today()
            return today.year - self.hire_date.year - (
                (today.month, today.day) < (self.hire_date.month, self.hire_date.day)
            )
        return 0


class AttendanceRecord(db.Model):
    __tablename__ = 'attendance_records'

    id = db.Column(db.Integer, primary_key=True)
    employee_id = db.Column(db.Integer, db.ForeignKey('employees.id'), nullable=False)
    date = db.Column(db.Date, nullable=False)
    check_in = db.Column(db.String(10))   # HH:MM
    check_out = db.Column(db.String(10))  # HH:MM
    status = db.Column(db.String(20), default='present')
    # present, absent, late, half_day, leave
    note = db.Column(db.String(255))
    created_at = db.Column(db.DateTime, default=db.func.now())

    __table_args__ = (
        db.UniqueConstraint('employee_id', 'date', name='uq_attendance_employee_date'),
    )

    def __repr__(self):
        return f'<Attendance {self.employee_id} {self.date}>'

    @property
    def work_hours(self):
        if self.check_in and self.check_out:
            try:
                cin = [int(x) for x in self.check_in.split(':')]
                cout = [int(x) for x in self.check_out.split(':')]
                minutes = (cout[0] * 60 + cout[1]) - (cin[0] * 60 + cin[1])
                return round(minutes / 60, 1)
            except (ValueError, IndexError):
                return 0
        return 0

    STATUS_LABELS = {
        'present': ('Có mặt', 'success'),
        'absent': ('Vắng mặt', 'danger'),
        'late': ('Đi muộn', 'warning'),
        'half_day': ('Nửa ngày', 'info'),
        'leave': ('Nghỉ phép', 'secondary'),
    }

    @property
    def status_label(self):
        return self.STATUS_LABELS.get(self.status, (self.status, 'light'))


class LeaveRequest(db.Model):
    __tablename__ = 'leave_requests'

    id = db.Column(db.Integer, primary_key=True)
    employee_id = db.Column(db.Integer, db.ForeignKey('employees.id'), nullable=False)
    leave_type = db.Column(db.String(50), nullable=False)
    # annual, sick, personal, unpaid
    start_date = db.Column(db.Date, nullable=False)
    end_date = db.Column(db.Date, nullable=False)
    reason = db.Column(db.String(500))
    status = db.Column(db.String(20), default='pending')  # pending, approved, rejected
    approved_by = db.Column(db.String(100))
    created_at = db.Column(db.DateTime, default=db.func.now())

    LEAVE_TYPES = {
        'annual': 'Nghỉ phép năm',
        'sick': 'Nghỉ bệnh',
        'personal': 'Nghỉ việc riêng',
        'unpaid': 'Nghỉ không lương',
        'maternity': 'Nghỉ thai sản',
    }

    STATUS_LABELS = {
        'pending': ('Chờ duyệt', 'warning'),
        'approved': ('Đã duyệt', 'success'),
        'rejected': ('Từ chối', 'danger'),
    }

    def __repr__(self):
        return f'<LeaveRequest {self.employee_id} {self.start_date}>'

    @property
    def days_count(self):
        if self.start_date and self.end_date:
            delta = self.end_date - self.start_date
            return delta.days + 1
        return 0

    @property
    def leave_type_label(self):
        return self.LEAVE_TYPES.get(self.leave_type, self.leave_type)

    @property
    def status_label(self):
        return self.STATUS_LABELS.get(self.status, (self.status, 'light'))


class PayrollRecord(db.Model):
    __tablename__ = 'payroll_records'

    id = db.Column(db.Integer, primary_key=True)
    employee_id = db.Column(db.Integer, db.ForeignKey('employees.id'), nullable=False)
    month = db.Column(db.Integer, nullable=False)   # 1-12
    year = db.Column(db.Integer, nullable=False)
    base_salary = db.Column(db.Float, default=0)
    allowance = db.Column(db.Float, default=0)       # Phụ cấp
    bonus = db.Column(db.Float, default=0)            # Thưởng
    deduction = db.Column(db.Float, default=0)        # Khấu trừ
    work_days = db.Column(db.Integer, default=0)
    standard_days = db.Column(db.Integer, default=26)
    status = db.Column(db.String(20), default='draft')  # draft, paid
    note = db.Column(db.String(500))
    created_at = db.Column(db.DateTime, default=db.func.now())

    __table_args__ = (
        db.UniqueConstraint('employee_id', 'month', 'year',
                            name='uq_payroll_employee_month_year'),
    )

    def __repr__(self):
        return f'<PayrollRecord {self.employee_id} {self.month}/{self.year}>'

    @property
    def actual_salary(self):
        if self.standard_days > 0:
            daily = self.base_salary / self.standard_days
            return round(daily * self.work_days, 0)
        return self.base_salary

    @property
    def gross_salary(self):
        return self.actual_salary + self.allowance + self.bonus

    @property
    def net_salary(self):
        return self.gross_salary - self.deduction

    @property
    def status_label(self):
        labels = {
            'draft': ('Nháp', 'secondary'),
            'paid': ('Đã thanh toán', 'success'),
        }
        return labels.get(self.status, (self.status, 'light'))
